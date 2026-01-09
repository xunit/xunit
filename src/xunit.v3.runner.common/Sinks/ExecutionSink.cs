using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// This is the execution sink which most runners will use, which can perform several operations
/// (including supporting result writers, detecting long running tests, failing skipped tests,
/// failing tests with warnings, and converting the top-level discovery and execution messages
/// into their runner counterparts).
/// </summary>
public class ExecutionSink : IMessageSink, IDisposable
{
	readonly AppDomainOption appDomainOption;
	readonly XunitProjectAssembly assembly;
	readonly ITestFrameworkDiscoveryOptions discoveryOptions;
	volatile int errors;
	readonly Dictionary<string, (ITestCaseMetadata TestCaseMetadata, DateTimeOffset StartTime)>? executingTestCases;
	readonly ITestFrameworkExecutionOptions executionOptions;
	readonly Dictionary<string, int> failCountsByUniqueID = [];
	readonly IMessageSink innerSink;
	readonly ExecutionSinkOptions options;
	DateTimeOffset lastTestActivity;
	readonly bool shadowCopy;
#pragma warning disable CA2213  // This object is owned by the creator, not this class
	readonly ISourceInformationProvider sourceInformationProvider;
#pragma warning restore CA2213
	ManualResetEvent? stopEvent;
	bool stopRequested;
	ManualResetEvent? workerFinished;

	/// <summary>
	/// Initializes a new instance of the <see cref="ExecutionSink"/> class.
	/// </summary>
	/// <param name="assembly">The assembly under test.</param>
	/// <param name="discoveryOptions">The options used during test discovery.</param>
	/// <param name="executionOptions">The options used during test execution.</param>
	/// <param name="appDomainOption">A flag to indicate whether app domains are in use.</param>
	/// <param name="shadowCopy">A flag to indicate whether shadow copying is in use.</param>
	/// <param name="innerSink">The inner sink to forward messages to (typically the reporter
	/// message handler, retrieved by calling <see cref="IRunnerReporter.CreateMessageHandler"/>
	/// on the runner reporter)</param>
	/// <param name="options">The options to use for the execution sink</param>
	public ExecutionSink(
		XunitProjectAssembly assembly,
		ITestFrameworkDiscoveryOptions discoveryOptions,
		ITestFrameworkExecutionOptions executionOptions,
		AppDomainOption appDomainOption,
		bool shadowCopy,
		IMessageSink innerSink,
		ExecutionSinkOptions options) :
			this(assembly, discoveryOptions, executionOptions, appDomainOption, shadowCopy, innerSink, options, NullSourceInformationProvider.Instance)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="ExecutionSink"/> class.
	/// </summary>
	/// <param name="assembly">The assembly under test.</param>
	/// <param name="discoveryOptions">The options used during test discovery.</param>
	/// <param name="executionOptions">The options used during test execution.</param>
	/// <param name="appDomainOption">A flag to indicate whether app domains are in use.</param>
	/// <param name="shadowCopy">A flag to indicate whether shadow copying is in use.</param>
	/// <param name="innerSink">The inner sink to forward messages to (typically the reporter
	/// message handler, retrieved by calling <see cref="IRunnerReporter.CreateMessageHandler"/>
	/// on the runner reporter)</param>
	/// <param name="options">The options to use for the execution sink</param>
	/// <param name="sourceInformationProvider">The source information provider</param>
	public ExecutionSink(
		XunitProjectAssembly assembly,
		ITestFrameworkDiscoveryOptions discoveryOptions,
		ITestFrameworkExecutionOptions executionOptions,
		AppDomainOption appDomainOption,
		bool shadowCopy,
		IMessageSink innerSink,
		ExecutionSinkOptions options,
		ISourceInformationProvider sourceInformationProvider)
	{
		this.assembly = Guard.ArgumentNotNull(assembly);
		this.discoveryOptions = Guard.ArgumentNotNull(discoveryOptions);
		this.executionOptions = Guard.ArgumentNotNull(executionOptions);
		this.appDomainOption = appDomainOption;
		this.shadowCopy = shadowCopy;
		this.innerSink = Guard.ArgumentNotNull(innerSink);
		this.options = Guard.ArgumentNotNull(options);
		this.sourceInformationProvider = Guard.ArgumentNotNull(sourceInformationProvider);

		if (options.LongRunningTestTime > TimeSpan.Zero && !Debugger.IsAttached)
			executingTestCases = [];
	}

	/// <inheritdoc/>
	public ExecutionSummary ExecutionSummary { get; } = new();

	/// <inheritdoc/>
	public ManualResetEvent Finished { get; } = new(initialState: false);

	/// <summary>
	/// Returns the current time in UTC. Overrideable for testing purposes.
	/// </summary>
	protected virtual DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

	void ConvertToRunnerMessageAndDispatch(IMessageSinkMessage message)
	{
		if (message is IDiscoveryComplete discoveryComplete)
			innerSink.OnMessage(new TestAssemblyDiscoveryFinished
			{
				Assembly = assembly,
				DiscoveryOptions = discoveryOptions,
				TestCasesToRun = discoveryComplete.TestCasesToRun,
				UniqueID = discoveryComplete.AssemblyUniqueID,
			});
		else if (message is IDiscoveryStarting discoveryStarting)
			innerSink.OnMessage(new TestAssemblyDiscoveryStarting
			{
				AppDomain = appDomainOption,
				Assembly = assembly,
				DiscoveryOptions = discoveryOptions,
				ShadowCopy = shadowCopy,
				UniqueID = discoveryStarting.AssemblyUniqueID,
			});
		else if (message is ITestAssemblyFinished assemblyFinished)
			innerSink.OnMessage(new TestAssemblyExecutionFinished
			{
				Assembly = assembly,
				ExecutionOptions = executionOptions,
				ExecutionSummary = ExecutionSummary,
				UniqueID = assemblyFinished.AssemblyUniqueID,
			});
		else if (message is ITestAssemblyStarting testAssemblyStarting)
			innerSink.OnMessage(new TestAssemblyExecutionStarting
			{
				Assembly = assembly,
				ExecutionOptions = executionOptions,
				Seed = testAssemblyStarting.Seed,
				UniqueID = testAssemblyStarting.UniqueID,
			});
	}

	/// <inheritdoc/>
	public virtual void Dispose()
	{
		GC.SuppressFinalize(this);

		// Make sure the timeout worker is finished, it may be sitting waiting past the time when the final
		// message has been delivered.
		Finished.SafeSet();
		stopEvent?.SafeSet();
		workerFinished?.WaitOne();

		Finished.SafeDispose();
		stopEvent?.SafeDispose();
		workerFinished?.SafeDispose();
	}

	void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args) =>
		Interlocked.Increment(ref errors);

	void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<ITestAssemblyCleanupFailure> args) =>
		Interlocked.Increment(ref errors);

	void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
	{
		// Avoid attempting to double-report test assembly finished (from crash detection)
		if (stopRequested)
			return;

		ExecutionSummary.Errors = errors;
		ExecutionSummary.Failed = args.Message.TestsFailed;
		ExecutionSummary.NotRun = args.Message.TestsNotRun;
		ExecutionSummary.Skipped = args.Message.TestsSkipped;
		ExecutionSummary.Time = args.Message.ExecutionTime;
		ExecutionSummary.Total = args.Message.TestsTotal;

		options.FinishedCallback?.Invoke(ExecutionSummary);

		stopRequested = true;
	}

	void HandleTestAssemblyStarting(MessageHandlerArgs<ITestAssemblyStarting> args)
	{
		if (executingTestCases is not null)
		{
			stopEvent = new ManualResetEvent(initialState: false);
			workerFinished = new ManualResetEvent(initialState: false);
			lastTestActivity = UtcNow;
			ThreadPool.QueueUserWorkItem(ThreadWorker);
		}
	}

	void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args) =>
		Interlocked.Increment(ref errors);

	void HandleTestCaseFinished(MessageHandlerArgs<ITestCaseFinished> args)
	{
		if (executingTestCases is not null)
			lock (executingTestCases)
				executingTestCases.Remove(args.Message.TestCaseUniqueID);
	}

	void HandleTestCaseStarting(MessageHandlerArgs<ITestCaseStarting> args)
	{
		if (executingTestCases is not null)
			lock (executingTestCases)
				executingTestCases.Add(args.Message.TestCaseUniqueID, (args.Message, UtcNow));
	}

	void HandleTestClassCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args) =>
		Interlocked.Increment(ref errors);

	void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args) =>
		Interlocked.Increment(ref errors);

	void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args) =>
		Interlocked.Increment(ref errors);

	void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args) =>
		Interlocked.Increment(ref errors);

	static IMessageSinkMessage MutateForFailSkips(IMessageSinkMessage message) =>
		message switch
		{
			ITestSkipped testSkipped => new TestFailed
			{
				AssemblyUniqueID = testSkipped.AssemblyUniqueID,
				Cause = FailureCause.Other,
				ExceptionParentIndices = [-1],
				ExceptionTypes = ["FAIL_SKIP"],
				ExecutionTime = 0m,
				FinishTime = testSkipped.FinishTime,
				Messages = [testSkipped.Reason],
				Output = "",
				StackTraces = [""],
				TestCaseUniqueID = testSkipped.TestCaseUniqueID,
				TestClassUniqueID = testSkipped.TestClassUniqueID,
				TestCollectionUniqueID = testSkipped.TestCollectionUniqueID,
				TestMethodUniqueID = testSkipped.TestMethodUniqueID,
				TestUniqueID = testSkipped.TestUniqueID,
				Warnings = testSkipped.Warnings,
			},
			ITestCaseFinished testCaseFinished => new TestCaseFinished
			{
				AssemblyUniqueID = testCaseFinished.AssemblyUniqueID,
				ExecutionTime = testCaseFinished.ExecutionTime,
				FinishTime = testCaseFinished.FinishTime,
				TestCaseUniqueID = testCaseFinished.TestCaseUniqueID,
				TestClassUniqueID = testCaseFinished.TestClassUniqueID,
				TestCollectionUniqueID = testCaseFinished.TestCollectionUniqueID,
				TestMethodUniqueID = testCaseFinished.TestMethodUniqueID,
				TestsFailed = testCaseFinished.TestsFailed + testCaseFinished.TestsSkipped,
				TestsNotRun = testCaseFinished.TestsNotRun,
				TestsTotal = testCaseFinished.TestsTotal,
				TestsSkipped = 0,
			},
			ITestMethodFinished testMethodFinished => new TestMethodFinished
			{
				AssemblyUniqueID = testMethodFinished.AssemblyUniqueID,
				ExecutionTime = testMethodFinished.ExecutionTime,
				FinishTime = testMethodFinished.FinishTime,
				TestClassUniqueID = testMethodFinished.TestClassUniqueID,
				TestCollectionUniqueID = testMethodFinished.TestCollectionUniqueID,
				TestMethodUniqueID = testMethodFinished.TestMethodUniqueID,
				TestsFailed = testMethodFinished.TestsFailed + testMethodFinished.TestsSkipped,
				TestsNotRun = testMethodFinished.TestsNotRun,
				TestsTotal = testMethodFinished.TestsTotal,
				TestsSkipped = 0,
			},
			ITestClassFinished testClassFinished => new TestClassFinished
			{
				AssemblyUniqueID = testClassFinished.AssemblyUniqueID,
				ExecutionTime = testClassFinished.ExecutionTime,
				FinishTime = testClassFinished.FinishTime,
				TestClassUniqueID = testClassFinished.TestClassUniqueID,
				TestCollectionUniqueID = testClassFinished.TestCollectionUniqueID,
				TestsFailed = testClassFinished.TestsFailed + testClassFinished.TestsSkipped,
				TestsNotRun = testClassFinished.TestsNotRun,
				TestsTotal = testClassFinished.TestsTotal,
				TestsSkipped = 0,
			},
			ITestCollectionFinished testCollectionFinished => new TestCollectionFinished
			{
				AssemblyUniqueID = testCollectionFinished.AssemblyUniqueID,
				ExecutionTime = testCollectionFinished.ExecutionTime,
				FinishTime = testCollectionFinished.FinishTime,
				TestCollectionUniqueID = testCollectionFinished.TestCollectionUniqueID,
				TestsFailed = testCollectionFinished.TestsFailed + testCollectionFinished.TestsSkipped,
				TestsNotRun = testCollectionFinished.TestsNotRun,
				TestsTotal = testCollectionFinished.TestsTotal,
				TestsSkipped = 0,
			},
			ITestAssemblyFinished assemblyFinished => new TestAssemblyFinished
			{
				AssemblyUniqueID = assemblyFinished.AssemblyUniqueID,
				ExecutionTime = assemblyFinished.ExecutionTime,
				FinishTime = assemblyFinished.FinishTime,
				TestsFailed = assemblyFinished.TestsFailed + assemblyFinished.TestsSkipped,
				TestsNotRun = assemblyFinished.TestsNotRun,
				TestsTotal = assemblyFinished.TestsTotal,
				TestsSkipped = 0,
			},
			_ => message,
		};

	IMessageSinkMessage MutateForFailWarn(IMessageSinkMessage message)
	{
		if (message is ITestPassed testPassed && testPassed.Warnings?.Length > 0)
		{
			lock (failCountsByUniqueID)
			{
				failCountsByUniqueID[testPassed.TestCaseUniqueID] = failCountsByUniqueID.AddOrGet(testPassed.TestCaseUniqueID) + 1;
				if (testPassed.TestMethodUniqueID is not null)
					failCountsByUniqueID[testPassed.TestMethodUniqueID] = failCountsByUniqueID.AddOrGet(testPassed.TestMethodUniqueID) + 1;
				if (testPassed.TestClassUniqueID is not null)
					failCountsByUniqueID[testPassed.TestClassUniqueID] = failCountsByUniqueID.AddOrGet(testPassed.TestClassUniqueID) + 1;
				failCountsByUniqueID[testPassed.TestCollectionUniqueID] = failCountsByUniqueID.AddOrGet(testPassed.TestCollectionUniqueID) + 1;
				failCountsByUniqueID[testPassed.AssemblyUniqueID] = failCountsByUniqueID.AddOrGet(testPassed.AssemblyUniqueID) + 1;
			}

			return new TestFailed
			{
				AssemblyUniqueID = testPassed.AssemblyUniqueID,
				Cause = FailureCause.Other,
				ExceptionParentIndices = [-1],
				ExceptionTypes = ["FAIL_WARN"],
				ExecutionTime = testPassed.ExecutionTime,
				FinishTime = testPassed.FinishTime,
				Messages = ["This test failed due to one or more warnings"],
				Output = testPassed.Output,
				StackTraces = [""],
				TestCaseUniqueID = testPassed.TestCaseUniqueID,
				TestClassUniqueID = testPassed.TestClassUniqueID,
				TestCollectionUniqueID = testPassed.TestCollectionUniqueID,
				TestMethodUniqueID = testPassed.TestMethodUniqueID,
				TestUniqueID = testPassed.TestUniqueID,
				Warnings = testPassed.Warnings,
			};
		}

		if (message is ITestCaseFinished testCaseFinished)
		{
			int failedByCase;

			lock (failCountsByUniqueID)
				if (!failCountsByUniqueID.TryGetValue(testCaseFinished.TestCaseUniqueID, out failedByCase))
					failedByCase = 0;

			return new TestCaseFinished
			{
				AssemblyUniqueID = testCaseFinished.AssemblyUniqueID,
				ExecutionTime = testCaseFinished.ExecutionTime,
				FinishTime = testCaseFinished.FinishTime,
				TestCaseUniqueID = testCaseFinished.TestCaseUniqueID,
				TestClassUniqueID = testCaseFinished.TestClassUniqueID,
				TestCollectionUniqueID = testCaseFinished.TestCollectionUniqueID,
				TestMethodUniqueID = testCaseFinished.TestMethodUniqueID,
				TestsFailed = testCaseFinished.TestsFailed + failedByCase,
				TestsNotRun = testCaseFinished.TestsNotRun,
				TestsTotal = testCaseFinished.TestsTotal,
				TestsSkipped = testCaseFinished.TestsSkipped,
			};
		}

		if (message is ITestMethodFinished testMethodFinished)
		{
			var failedByMethod = 0;

			if (testMethodFinished.TestMethodUniqueID is not null)
				lock (failCountsByUniqueID)
					if (!failCountsByUniqueID.TryGetValue(testMethodFinished.TestMethodUniqueID, out failedByMethod))
						failedByMethod = 0;

			return new TestMethodFinished
			{
				AssemblyUniqueID = testMethodFinished.AssemblyUniqueID,
				ExecutionTime = testMethodFinished.ExecutionTime,
				FinishTime = testMethodFinished.FinishTime,
				TestClassUniqueID = testMethodFinished.TestClassUniqueID,
				TestCollectionUniqueID = testMethodFinished.TestCollectionUniqueID,
				TestMethodUniqueID = testMethodFinished.TestMethodUniqueID,
				TestsFailed = testMethodFinished.TestsFailed + failedByMethod,
				TestsNotRun = testMethodFinished.TestsNotRun,
				TestsTotal = testMethodFinished.TestsTotal,
				TestsSkipped = testMethodFinished.TestsSkipped,
			};
		}

		if (message is ITestClassFinished testClassFinished)
		{
			var failedByClass = 0;

			if (testClassFinished.TestClassUniqueID is not null)
				lock (failCountsByUniqueID)
					if (!failCountsByUniqueID.TryGetValue(testClassFinished.TestClassUniqueID, out failedByClass))
						failedByClass = 0;

			return new TestClassFinished
			{
				AssemblyUniqueID = testClassFinished.AssemblyUniqueID,
				ExecutionTime = testClassFinished.ExecutionTime,
				FinishTime = testClassFinished.FinishTime,
				TestClassUniqueID = testClassFinished.TestClassUniqueID,
				TestCollectionUniqueID = testClassFinished.TestCollectionUniqueID,
				TestsFailed = testClassFinished.TestsFailed + failedByClass,
				TestsNotRun = testClassFinished.TestsNotRun,
				TestsTotal = testClassFinished.TestsTotal,
				TestsSkipped = testClassFinished.TestsSkipped,
			};
		}

		if (message is ITestCollectionFinished testCollectionFinished)
		{
			int failedByCollection;

			lock (failCountsByUniqueID)
				if (!failCountsByUniqueID.TryGetValue(testCollectionFinished.TestCollectionUniqueID, out failedByCollection))
					failedByCollection = 0;

			return new TestCollectionFinished
			{
				AssemblyUniqueID = testCollectionFinished.AssemblyUniqueID,
				ExecutionTime = testCollectionFinished.ExecutionTime,
				FinishTime = testCollectionFinished.FinishTime,
				TestCollectionUniqueID = testCollectionFinished.TestCollectionUniqueID,
				TestsFailed = testCollectionFinished.TestsFailed + failedByCollection,
				TestsNotRun = testCollectionFinished.TestsNotRun,
				TestsTotal = testCollectionFinished.TestsTotal,
				TestsSkipped = testCollectionFinished.TestsSkipped,
			};
		}

		if (message is ITestAssemblyFinished assemblyFinished)
		{
			int failedByAssembly;

			lock (failCountsByUniqueID)
				if (!failCountsByUniqueID.TryGetValue(assemblyFinished.AssemblyUniqueID, out failedByAssembly))
					failedByAssembly = 0;

			return new TestAssemblyFinished
			{
				AssemblyUniqueID = assemblyFinished.AssemblyUniqueID,
				ExecutionTime = assemblyFinished.ExecutionTime,
				FinishTime = assemblyFinished.FinishTime,
				TestsFailed = assemblyFinished.TestsFailed + failedByAssembly,
				TestsNotRun = assemblyFinished.TestsNotRun,
				TestsTotal = assemblyFinished.TestsTotal,
				TestsSkipped = assemblyFinished.TestsSkipped,
			};
		}

		return message;
	}

	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		// Mutate message for source information
		if (message is ITestCaseStarting testCaseStarting && testCaseStarting.SourceFilePath is null && testCaseStarting.SourceLineNumber is null)
			message = testCaseStarting.WithSourceInfo(sourceInformationProvider);

		// Mutate messages based on user requirements
		if (options.FailSkips)
			message = MutateForFailSkips(message);

		if (options.FailWarn)
			message = MutateForFailWarn(message);

		// General handlers
		var result =
			message.DispatchWhen<IErrorMessage>(HandleErrorMessage)
			&& message.DispatchWhen<ITestAssemblyCleanupFailure>(HandleTestAssemblyCleanupFailure)
			&& message.DispatchWhen<ITestAssemblyFinished>(HandleTestAssemblyFinished)
			&& message.DispatchWhen<ITestAssemblyStarting>(HandleTestAssemblyStarting)
			&& message.DispatchWhen<ITestCaseCleanupFailure>(HandleTestCaseCleanupFailure)
			&& message.DispatchWhen<ITestCaseFinished>(HandleTestCaseFinished)
			&& message.DispatchWhen<ITestCaseStarting>(HandleTestCaseStarting)
			&& message.DispatchWhen<ITestClassCleanupFailure>(HandleTestClassCleanupFailure)
			&& message.DispatchWhen<ITestCleanupFailure>(HandleTestCleanupFailure)
			&& message.DispatchWhen<ITestCollectionCleanupFailure>(HandleTestCollectionCleanupFailure)
			&& message.DispatchWhen<ITestMethodCleanupFailure>(HandleTestMethodCleanupFailure);

		// Do the message conversions last, since they consume values created by the general handlers
		ConvertToRunnerMessageAndDispatch(message);

		// Dispatch to the result writer message handlers
		if (options.ResultWriterMessageHandlers is not null)
			foreach (var resultWriter in options.ResultWriterMessageHandlers.WhereNotNull())
				result = resultWriter.OnMessage(message) && result;

		// Dispatch to the reporter handler
		result =
			innerSink.OnMessage(message)
			&& result
			&& (options.CancelThunk == null || !options.CancelThunk());

		// Don't request stop until after the inner handler has had a chance to process the message
		// per https://github.com/xunit/visualstudio.xunit/issues/396
		if (stopRequested)
		{
			Finished.SafeSet();
			stopEvent?.SafeSet();
		}

		return result;
	}

	void SendLongRunningMessage()
	{
		if (executingTestCases is null)
			return;

		Dictionary<ITestCaseMetadata, TimeSpan> longRunningTestCases;
		lock (executingTestCases)
		{
			var now = UtcNow;
			longRunningTestCases =
				executingTestCases
					.Where(kvp => (now - kvp.Value.StartTime) >= options.LongRunningTestTime)
					.ToDictionary(kvp => kvp.Value.TestCaseMetadata, kvp => now - kvp.Value.StartTime);
		}

		if (longRunningTestCases.Count > 0)
		{
			if (options.LongRunningTestCallback is not null)
				options.LongRunningTestCallback(new LongRunningTestsSummary(options.LongRunningTestTime, longRunningTestCases));

			options.DiagnosticMessageSink?.OnMessage(
				new DiagnosticMessage(
					string.Join(
						Environment.NewLine,
						longRunningTestCases.Select(pair => string.Format(CultureInfo.CurrentCulture, @"[Long Running Test] '{0}', Elapsed: {1:hh\:mm\:ss}", pair.Key.TestCaseDisplayName, pair.Value)).ToArray()
					)
				)
			);
		}
	}

	void ThreadWorker(object? _)
	{
		// Fire the loop approximately every 1/10th of our delay time, but no more frequently than every
		// second (so we don't over-fire the timer). This should give us reasonable precision for the
		// requested delay time, without going crazy to check for long-running tests.

		var delayTime = (int)Math.Max(1000, options.LongRunningTestTime.TotalMilliseconds / 10);

		while (true)
		{
			if (WaitForStopEvent(delayTime))
			{
				workerFinished?.SafeSet();
				return;
			}

			var now = UtcNow;
			if (now - lastTestActivity >= options.LongRunningTestTime)
			{
				SendLongRunningMessage();
				lastTestActivity = now;
			}
		}
	}

	/// <summary>
	/// Performs a Task-safe delay. Overrideable for testing purposes.
	/// </summary>
	protected virtual bool WaitForStopEvent(int millionsecondsDelay)
		=> stopEvent?.WaitOne(millionsecondsDelay) ?? true;
}
