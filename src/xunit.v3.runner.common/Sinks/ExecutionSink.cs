using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// This is the execution sink which most runners will use, which can perform several operations
/// (including recording XML results, detecting long running tests, failing skipped tests,
/// failing tests with warnings, and converting the top-level discovery and execution messages
/// into their runner counterparts).
/// </summary>
public class ExecutionSink : _IMessageSink, IDisposable
{
	readonly AppDomainOption appDomainOption;
	readonly XunitProjectAssembly assembly;
	readonly _ITestFrameworkDiscoveryOptions discoveryOptions;
	volatile int errors;
	readonly Lazy<XElement> errorsElement;
	readonly Dictionary<string, (_ITestCaseMetadata TestCaseMetadata, DateTimeOffset StartTime)>? executingTestCases;
	readonly _ITestFrameworkExecutionOptions executionOptions;
	readonly Dictionary<string, int> failCountsByUniqueID = [];
	readonly _IMessageSink innerSink;
	readonly ExecutionSinkOptions options;
	DateTimeOffset lastTestActivity;
	readonly MessageMetadataCache metadataCache = new();
	readonly bool shadowCopy;
	ManualResetEvent? stopEvent;
	bool stopRequested;

	readonly Dictionary<string, XElement> testCollectionElements = [];

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
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestFrameworkExecutionOptions executionOptions,
		AppDomainOption appDomainOption,
		bool shadowCopy,
		_IMessageSink innerSink,
		ExecutionSinkOptions options)
	{
		this.assembly = Guard.ArgumentNotNull(assembly);
		this.discoveryOptions = Guard.ArgumentNotNull(discoveryOptions);
		this.executionOptions = Guard.ArgumentNotNull(executionOptions);
		this.appDomainOption = appDomainOption;
		this.shadowCopy = shadowCopy;
		this.innerSink = Guard.ArgumentNotNull(innerSink);
		this.options = Guard.ArgumentNotNull(options);

		if (options.LongRunningTestTime > TimeSpan.Zero)
			executingTestCases = [];

		if (options.AssemblyElement is not null)
			errorsElement = new(() =>
			{
				var result = new XElement("errors");
				options.AssemblyElement.Add(result);
				return result;
			});
		else
			errorsElement = new(() => new XElement("errors"));
	}

	/// <inheritdoc/>
	public ExecutionSummary ExecutionSummary { get; } = new();

	/// <inheritdoc/>
	public ManualResetEvent Finished { get; } = new(initialState: false);

	/// <summary>
	/// Returns the current time in UTC. Overrideable for testing purposes.
	/// </summary>
	protected virtual DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

	void AddError(
		string type,
		string? name,
		_IErrorMetadata errorMetadata)
	{
		var errorElement = new XElement(
			"error",
			new XAttribute("type", type),
			CreateFailureElement(errorMetadata)
		);

		if (name is not null)
			errorElement.Add(new XAttribute("name", name));

		errorsElement.Value.Add(errorElement);
	}

	void ConvertToRunnerMessageAndDispatch(_MessageSinkMessage message)
	{
		if (message is _DiscoveryComplete discoveryComplete)
			innerSink.OnMessage(new TestAssemblyDiscoveryFinished
			{
				Assembly = assembly,
				DiscoveryOptions = discoveryOptions,
				TestCasesToRun = discoveryComplete.TestCasesToRun,
			});
		else if (message is _DiscoveryStarting)
			innerSink.OnMessage(new TestAssemblyDiscoveryStarting
			{
				AppDomain = appDomainOption,
				Assembly = assembly,
				DiscoveryOptions = discoveryOptions,
				ShadowCopy = shadowCopy,
			});
		else if (message is _TestAssemblyFinished)
			innerSink.OnMessage(new TestAssemblyExecutionFinished
			{
				Assembly = assembly,
				ExecutionOptions = executionOptions,
				ExecutionSummary = ExecutionSummary,
			});
		else if (message is _TestAssemblyStarting testAssemblyStarting)
			innerSink.OnMessage(new TestAssemblyExecutionStarting
			{
				Assembly = assembly,
				ExecutionOptions = executionOptions,
				Seed = testAssemblyStarting.Seed,
			});
	}

	static XElement CreateFailureElement(_IErrorMetadata errorMetadata)
	{
		var result = new XElement("failure");

		var exceptionType = errorMetadata.ExceptionTypes[0];
		if (exceptionType is not null)
			result.Add(new XAttribute("exception-type", exceptionType));

		var message = ExceptionUtility.CombineMessages(errorMetadata);
		if (!string.IsNullOrWhiteSpace(message))
			result.Add(new XElement("message", new XCData(XmlEscape(message))));

		var stackTrace = ExceptionUtility.CombineStackTraces(errorMetadata);
		if (stackTrace is not null)
			result.Add(new XElement("stack-trace", new XCData(stackTrace)));

		return result;
	}

	XElement CreateTestResultElement(
		_TestResultMessage testResult,
		string resultText)
	{
		var testMetadata = Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Cannot find test metadata for ID {0}", testResult.TestUniqueID), metadataCache.TryGetTestMetadata(testResult));
		var testCaseMetadata = Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Cannot find test case metadata for ID {0}", testResult.TestCaseUniqueID), metadataCache.TryGetTestCaseMetadata(testResult));
		var testMethodMetadata = metadataCache.TryGetMethodMetadata(testResult);
		var testClassMetadata = metadataCache.TryGetClassMetadata(testResult);

		var collectionElement = GetTestCollectionElement(testResult.TestCollectionUniqueID);
		var testResultElement =
			new XElement("test",
				new XAttribute("id", Guid.NewGuid().ToString("d")),
				new XAttribute("name", XmlEscape(testMetadata.TestDisplayName)),
				new XAttribute("result", resultText),
				new XAttribute("time", testResult.ExecutionTime.ToString(CultureInfo.InvariantCulture)),
				new XAttribute("time-rtf", TimeSpan.FromSeconds((double)testResult.ExecutionTime).ToString("c", CultureInfo.InvariantCulture))
			);

		var type = testClassMetadata?.TestClassName;
		if (type is not null)
			testResultElement.Add(new XAttribute("type", type));

		var method = testMethodMetadata?.MethodName;
		if (method is not null)
			testResultElement.Add(new XAttribute("method", method));

		var testOutput = testResult.Output;
		if (!string.IsNullOrWhiteSpace(testOutput))
			testResultElement.Add(new XElement("output", new XCData(testOutput)));

		if (testResult.Warnings is not null && testResult.Warnings.Length > 0)
		{
			var warningsElement = new XElement("warnings");

			foreach (var warning in testResult.Warnings)
				warningsElement.Add(new XElement("warning", new XCData(warning)));

			testResultElement.Add(warningsElement);
		}

		var fileName = testCaseMetadata.SourceFilePath;
		if (fileName is not null)
			testResultElement.Add(new XAttribute("source-file", fileName));

		var lineNumber = testCaseMetadata.SourceLineNumber;
		if (lineNumber is not null)
			testResultElement.Add(new XAttribute("source-line", lineNumber.GetValueOrDefault()));

		var traits = testCaseMetadata.Traits;
		if (traits is not null && traits.Count > 0)
		{
			var traitsElement = new XElement("traits");

			foreach (var keyValuePair in traits)
				foreach (var val in keyValuePair.Value)
					traitsElement.Add(
						new XElement("trait",
							new XAttribute("name", XmlEscape(keyValuePair.Key)),
							new XAttribute("value", XmlEscape(val))
						)
					);

			testResultElement.Add(traitsElement);
		}

		collectionElement.Add(testResultElement);

		return testResultElement;
	}

	/// <inheritdoc/>
	public virtual void Dispose()
	{
		GC.SuppressFinalize(this);

		Finished.Dispose();
		stopEvent?.Dispose();
	}

	XElement GetTestCollectionElement(string testCollectionUniqueID)
	{
		lock (testCollectionElements)
			return testCollectionElements.AddOrGet(testCollectionUniqueID, () => new XElement("collection"));
	}

	void HandleErrorMessage(MessageHandlerArgs<_ErrorMessage> args)
	{
		Interlocked.Increment(ref errors);

		if (options.AssemblyElement is not null)
			AddError("fatal", null, args.Message);
	}

	void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<_TestAssemblyCleanupFailure> args)
	{
		Interlocked.Increment(ref errors);

		if (options.AssemblyElement is not null)
			AddError("assembly-cleanup", metadataCache.TryGetAssemblyMetadata(args.Message)?.AssemblyPath, args.Message);
	}

	void HandleTestAssemblyFinished(MessageHandlerArgs<_TestAssemblyFinished> args)
	{
		ExecutionSummary.Errors = errors;
		ExecutionSummary.Failed = args.Message.TestsFailed;
		ExecutionSummary.NotRun = args.Message.TestsNotRun;
		ExecutionSummary.Skipped = args.Message.TestsSkipped;
		ExecutionSummary.Time = args.Message.ExecutionTime;
		ExecutionSummary.Total = args.Message.TestsTotal;

		options.FinishedCallback?.Invoke(ExecutionSummary);

		if (options.AssemblyElement is not null)
		{
			options.AssemblyElement.Add(
				new XAttribute("errors", ExecutionSummary.Errors),
				new XAttribute("failed", ExecutionSummary.Failed),
				new XAttribute("finish-rtf", args.Message.FinishTime.ToString("O", CultureInfo.InvariantCulture)),
				new XAttribute("not-run", ExecutionSummary.NotRun),
				new XAttribute("passed", ExecutionSummary.Total - ExecutionSummary.Failed - ExecutionSummary.Skipped - ExecutionSummary.NotRun),
				new XAttribute("skipped", ExecutionSummary.Skipped),
				new XAttribute("time", ExecutionSummary.Time.ToString("0.000", CultureInfo.InvariantCulture)),
				new XAttribute("time-rtf", TimeSpan.FromSeconds((double)ExecutionSummary.Time).ToString("c", CultureInfo.InvariantCulture)),
				new XAttribute("total", ExecutionSummary.Total)
			);

			foreach (var element in testCollectionElements.Values)
				options.AssemblyElement.Add(element);

			metadataCache.TryRemove(args.Message);
		}

		stopRequested = true;
	}

	void HandleTestAssemblyStarting(MessageHandlerArgs<_TestAssemblyStarting> args)
	{
		if (executingTestCases is not null)
		{
			stopEvent = new ManualResetEvent(initialState: false);
			lastTestActivity = UtcNow;
			ThreadPool.QueueUserWorkItem(ThreadWorker);
		}

		if (options.AssemblyElement is not null)
		{
			var assemblyStarting = args.Message;

			options.AssemblyElement.Add(
				new XAttribute("environment", assemblyStarting.TestEnvironment),
				new XAttribute("id", Guid.NewGuid().ToString("d")),
				new XAttribute("name", assemblyStarting.AssemblyPath ?? "<dynamic>"),
				new XAttribute("run-date", assemblyStarting.StartTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
				new XAttribute("run-time", assemblyStarting.StartTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture)),
				new XAttribute("start-rtf", assemblyStarting.StartTime.ToString("O", CultureInfo.InvariantCulture)),
				new XAttribute("test-framework", assemblyStarting.TestFrameworkDisplayName)
			);

			if (assemblyStarting.ConfigFilePath is not null)
				options.AssemblyElement.Add(new XAttribute("config-file", assemblyStarting.ConfigFilePath));
			if (assemblyStarting.TargetFramework is not null)
				options.AssemblyElement.Add(new XAttribute("target-framework", assemblyStarting.TargetFramework));

			metadataCache.Set(assemblyStarting);
		}
	}

	void HandleTestCaseCleanupFailure(MessageHandlerArgs<_TestCaseCleanupFailure> args)
	{
		Interlocked.Increment(ref errors);

		if (options.AssemblyElement is not null)
			AddError("test-case-cleanup", metadataCache.TryGetTestCaseMetadata(args.Message)?.TestCaseDisplayName, args.Message);
	}

	void HandleTestCaseFinished(MessageHandlerArgs<_TestCaseFinished> args)
	{
		if (executingTestCases is not null)
			lock (executingTestCases)
			{
				executingTestCases.Remove(args.Message.TestCaseUniqueID);
				lastTestActivity = UtcNow;
			}

		if (options.AssemblyElement is not null)
			metadataCache.TryRemove(args.Message);
	}

	void HandleTestCaseStarting(MessageHandlerArgs<_TestCaseStarting> args)
	{
		if (executingTestCases is not null)
			lock (executingTestCases)
				executingTestCases.Add(args.Message.TestCaseUniqueID, (args.Message, UtcNow));

		if (options.AssemblyElement is not null)
			metadataCache.Set(args.Message);
	}

	void HandleTestClassCleanupFailure(MessageHandlerArgs<_TestClassCleanupFailure> args)
	{
		Interlocked.Increment(ref errors);

		if (options.AssemblyElement is not null)
			AddError("test-class-cleanup", metadataCache.TryGetClassMetadata(args.Message)?.TestClassName, args.Message);
	}

	void HandleTestClassFinished(MessageHandlerArgs<_TestClassFinished> args)
	{
		if (options.AssemblyElement is not null)
			metadataCache.TryRemove(args.Message);
	}

	void HandleTestClassStarting(MessageHandlerArgs<_TestClassStarting> args)
	{
		if (options.AssemblyElement is not null)
			metadataCache.Set(args.Message);
	}

	void HandleTestCleanupFailure(MessageHandlerArgs<_TestCleanupFailure> args)
	{
		Interlocked.Increment(ref errors);

		if (options.AssemblyElement is not null)
			AddError("test-cleanup", metadataCache.TryGetTestMetadata(args.Message)?.TestDisplayName, args.Message);
	}

	void HandleTestCollectionCleanupFailure(MessageHandlerArgs<_TestCollectionCleanupFailure> args)
	{
		Interlocked.Increment(ref errors);

		if (options.AssemblyElement is not null)
			AddError("test-collection-cleanup", metadataCache.TryGetCollectionMetadata(args.Message)?.TestCollectionDisplayName, args.Message);
	}

	void HandleTestCollectionFinished(MessageHandlerArgs<_TestCollectionFinished> args)
	{
		if (options.AssemblyElement is not null)
		{
			var testCollectionFinished = args.Message;
			var collectionElement = GetTestCollectionElement(testCollectionFinished.TestCollectionUniqueID);

			collectionElement.Add(
				new XAttribute("failed", testCollectionFinished.TestsFailed),
				new XAttribute("not-run", testCollectionFinished.TestsNotRun),
				new XAttribute("passed", testCollectionFinished.TestsTotal - testCollectionFinished.TestsFailed - testCollectionFinished.TestsSkipped - testCollectionFinished.TestsNotRun),
				new XAttribute("skipped", testCollectionFinished.TestsSkipped),
				new XAttribute("time", testCollectionFinished.ExecutionTime.ToString("0.000", CultureInfo.InvariantCulture)),
				new XAttribute("time-rtf", TimeSpan.FromSeconds((double)testCollectionFinished.ExecutionTime).ToString("c", CultureInfo.InvariantCulture)),
				new XAttribute("total", testCollectionFinished.TestsTotal)
			);

			metadataCache.TryRemove(testCollectionFinished);
		}
	}

	void HandleTestCollectionStarting(MessageHandlerArgs<_TestCollectionStarting> args)
	{
		if (options.AssemblyElement is not null)
		{
			var testCollectionStarting = args.Message;
			var collectionElement = GetTestCollectionElement(testCollectionStarting.TestCollectionUniqueID);

			collectionElement.Add(
				new XAttribute("name", XmlEscape(testCollectionStarting.TestCollectionDisplayName)),
				new XAttribute("id", Guid.NewGuid().ToString("d"))
			);

			metadataCache.Set(testCollectionStarting);
		}
	}

	void HandleTestFailed(MessageHandlerArgs<_TestFailed> args)
	{
		if (options.AssemblyElement is not null)
		{
			var testFailed = args.Message;
			var testElement = CreateTestResultElement(testFailed, "Fail");

			testElement.Add(CreateFailureElement(testFailed));
		}
	}

	void HandleTestFinished(MessageHandlerArgs<_TestFinished> args)
	{
		if (options.AssemblyElement is not null)
			metadataCache.TryRemove(args.Message);
	}

	void HandleTestMethodCleanupFailure(MessageHandlerArgs<_TestMethodCleanupFailure> args)
	{
		Interlocked.Increment(ref errors);

		if (options.AssemblyElement is not null)
			AddError("test-method-cleanup", metadataCache.TryGetMethodMetadata(args.Message)?.MethodName, args.Message);
	}

	void HandleTestMethodFinished(MessageHandlerArgs<_TestMethodFinished> args)
	{
		if (options.AssemblyElement is not null)
			metadataCache.TryRemove(args.Message);
	}

	void HandleTestMethodStarting(MessageHandlerArgs<_TestMethodStarting> args)
	{
		if (options.AssemblyElement is not null)
			metadataCache.Set(args.Message);
	}

	void HandleTestNotRun(MessageHandlerArgs<_TestNotRun> args)
	{
		if (options.AssemblyElement is not null)
			CreateTestResultElement(args.Message, "NotRun");
	}

	void HandleTestPassed(MessageHandlerArgs<_TestPassed> args)
	{
		if (options.AssemblyElement is not null)
			CreateTestResultElement(args.Message, "Pass");
	}

	void HandleTestSkipped(MessageHandlerArgs<_TestSkipped> args)
	{
		if (options.AssemblyElement is not null)
		{
			var testSkipped = args.Message;
			var testElement = CreateTestResultElement(testSkipped, "Skip");

			testElement.Add(new XElement("reason", new XCData(XmlEscape(testSkipped.Reason))));
		}
	}

	void HandleTestStarting(MessageHandlerArgs<_TestStarting> args)
	{
		if (options.AssemblyElement is not null)
			metadataCache.Set(args.Message);
	}

	static _MessageSinkMessage MutateForFailSkips(_MessageSinkMessage message)
	{
		if (message is _TestSkipped testSkipped)
			return new _TestFailed
			{
				AssemblyUniqueID = testSkipped.AssemblyUniqueID,
				Cause = FailureCause.Other,
				ExceptionParentIndices = [-1],
				ExceptionTypes = ["FAIL_SKIP"],
				ExecutionTime = 0m,
				Messages = [testSkipped.Reason],
				Output = "",
				StackTraces = [""],
				TestCaseUniqueID = testSkipped.TestCaseUniqueID,
				TestClassUniqueID = testSkipped.TestClassUniqueID,
				TestCollectionUniqueID = testSkipped.TestCollectionUniqueID,
				TestMethodUniqueID = testSkipped.TestMethodUniqueID,
				TestUniqueID = testSkipped.TestUniqueID,
				Warnings = testSkipped.Warnings,
			};

		if (message is _TestCaseFinished testCaseFinished)
			return new _TestCaseFinished
			{
				AssemblyUniqueID = testCaseFinished.AssemblyUniqueID,
				ExecutionTime = testCaseFinished.ExecutionTime,
				TestCaseUniqueID = testCaseFinished.TestCaseUniqueID,
				TestClassUniqueID = testCaseFinished.TestClassUniqueID,
				TestCollectionUniqueID = testCaseFinished.TestCollectionUniqueID,
				TestMethodUniqueID = testCaseFinished.TestMethodUniqueID,
				TestsFailed = testCaseFinished.TestsFailed + testCaseFinished.TestsSkipped,
				TestsNotRun = testCaseFinished.TestsNotRun,
				TestsTotal = testCaseFinished.TestsTotal,
				TestsSkipped = 0,
			};

		if (message is _TestMethodFinished testMethodFinished)
			return new _TestMethodFinished
			{
				AssemblyUniqueID = testMethodFinished.AssemblyUniqueID,
				ExecutionTime = testMethodFinished.ExecutionTime,
				TestClassUniqueID = testMethodFinished.TestClassUniqueID,
				TestCollectionUniqueID = testMethodFinished.TestCollectionUniqueID,
				TestMethodUniqueID = testMethodFinished.TestMethodUniqueID,
				TestsFailed = testMethodFinished.TestsFailed + testMethodFinished.TestsSkipped,
				TestsNotRun = testMethodFinished.TestsNotRun,
				TestsTotal = testMethodFinished.TestsTotal,
				TestsSkipped = 0,
			};

		if (message is _TestClassFinished testClassFinished)
			return new _TestClassFinished
			{
				AssemblyUniqueID = testClassFinished.AssemblyUniqueID,
				ExecutionTime = testClassFinished.ExecutionTime,
				TestClassUniqueID = testClassFinished.TestClassUniqueID,
				TestCollectionUniqueID = testClassFinished.TestCollectionUniqueID,
				TestsFailed = testClassFinished.TestsFailed + testClassFinished.TestsSkipped,
				TestsNotRun = testClassFinished.TestsNotRun,
				TestsTotal = testClassFinished.TestsTotal,
				TestsSkipped = 0,
			};

		if (message is _TestCollectionFinished testCollectionFinished)
			return new _TestCollectionFinished
			{
				AssemblyUniqueID = testCollectionFinished.AssemblyUniqueID,
				ExecutionTime = testCollectionFinished.ExecutionTime,
				TestCollectionUniqueID = testCollectionFinished.TestCollectionUniqueID,
				TestsFailed = testCollectionFinished.TestsFailed + testCollectionFinished.TestsSkipped,
				TestsNotRun = testCollectionFinished.TestsNotRun,
				TestsTotal = testCollectionFinished.TestsTotal,
				TestsSkipped = 0,
			};

		if (message is _TestAssemblyFinished assemblyFinished)
			return new _TestAssemblyFinished
			{
				AssemblyUniqueID = assemblyFinished.AssemblyUniqueID,
				ExecutionTime = assemblyFinished.ExecutionTime,
				FinishTime = assemblyFinished.FinishTime,
				TestsFailed = assemblyFinished.TestsFailed + assemblyFinished.TestsSkipped,
				TestsNotRun = assemblyFinished.TestsNotRun,
				TestsTotal = assemblyFinished.TestsTotal,
				TestsSkipped = 0,
			};

		return message;
	}

	_MessageSinkMessage MutateForFailWarn(_MessageSinkMessage message)
	{
		if (message is _TestPassed testPassed && testPassed.Warnings?.Length > 0)
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

			return new _TestFailed
			{
				AssemblyUniqueID = testPassed.AssemblyUniqueID,
				Cause = FailureCause.Other,
				ExceptionParentIndices = [-1],
				ExceptionTypes = ["FAIL_WARN"],
				ExecutionTime = testPassed.ExecutionTime,
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

		if (message is _TestCaseFinished testCaseFinished)
		{
			int failedByCase;

			lock (failCountsByUniqueID)
				if (!failCountsByUniqueID.TryGetValue(testCaseFinished.TestCaseUniqueID, out failedByCase))
					failedByCase = 0;

			return new _TestCaseFinished
			{
				AssemblyUniqueID = testCaseFinished.AssemblyUniqueID,
				ExecutionTime = testCaseFinished.ExecutionTime,
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

		if (message is _TestMethodFinished testMethodFinished)
		{
			int failedByMethod = 0;

			if (testMethodFinished.TestMethodUniqueID is not null)
				lock (failCountsByUniqueID)
					if (!failCountsByUniqueID.TryGetValue(testMethodFinished.TestMethodUniqueID, out failedByMethod))
						failedByMethod = 0;

			return new _TestMethodFinished
			{
				AssemblyUniqueID = testMethodFinished.AssemblyUniqueID,
				ExecutionTime = testMethodFinished.ExecutionTime,
				TestClassUniqueID = testMethodFinished.TestClassUniqueID,
				TestCollectionUniqueID = testMethodFinished.TestCollectionUniqueID,
				TestMethodUniqueID = testMethodFinished.TestMethodUniqueID,
				TestsFailed = testMethodFinished.TestsFailed + failedByMethod,
				TestsNotRun = testMethodFinished.TestsNotRun,
				TestsTotal = testMethodFinished.TestsTotal,
				TestsSkipped = testMethodFinished.TestsSkipped,
			};
		}

		if (message is _TestClassFinished testClassFinished)
		{
			int failedByClass = 0;

			if (testClassFinished.TestClassUniqueID is not null)
				lock (failCountsByUniqueID)
					if (!failCountsByUniqueID.TryGetValue(testClassFinished.TestClassUniqueID, out failedByClass))
						failedByClass = 0;

			return new _TestClassFinished
			{
				AssemblyUniqueID = testClassFinished.AssemblyUniqueID,
				ExecutionTime = testClassFinished.ExecutionTime,
				TestClassUniqueID = testClassFinished.TestClassUniqueID,
				TestCollectionUniqueID = testClassFinished.TestCollectionUniqueID,
				TestsFailed = testClassFinished.TestsFailed + failedByClass,
				TestsNotRun = testClassFinished.TestsNotRun,
				TestsTotal = testClassFinished.TestsTotal,
				TestsSkipped = testClassFinished.TestsSkipped,
			};
		}

		if (message is _TestCollectionFinished testCollectionFinished)
		{
			int failedByCollection;

			lock (failCountsByUniqueID)
				if (!failCountsByUniqueID.TryGetValue(testCollectionFinished.TestCollectionUniqueID, out failedByCollection))
					failedByCollection = 0;

			return new _TestCollectionFinished
			{
				AssemblyUniqueID = testCollectionFinished.AssemblyUniqueID,
				ExecutionTime = testCollectionFinished.ExecutionTime,
				TestCollectionUniqueID = testCollectionFinished.TestCollectionUniqueID,
				TestsFailed = testCollectionFinished.TestsFailed + failedByCollection,
				TestsNotRun = testCollectionFinished.TestsNotRun,
				TestsTotal = testCollectionFinished.TestsTotal,
				TestsSkipped = testCollectionFinished.TestsSkipped,
			};
		}

		if (message is _TestAssemblyFinished assemblyFinished)
		{
			int failedByAssembly;

			lock (failCountsByUniqueID)
				if (!failCountsByUniqueID.TryGetValue(assemblyFinished.AssemblyUniqueID, out failedByAssembly))
					failedByAssembly = 0;

			return new _TestAssemblyFinished
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
	public bool OnMessage(_MessageSinkMessage message)
	{
		// Mutate messages based on user requirements
		if (options.FailSkips)
			message = MutateForFailSkips(message);

		if (options.FailWarn)
			message = MutateForFailWarn(message);

		// General handlers
		var result =
			message.DispatchWhen<_ErrorMessage>(HandleErrorMessage)
			&& message.DispatchWhen<_TestAssemblyCleanupFailure>(HandleTestAssemblyCleanupFailure)
			&& message.DispatchWhen<_TestAssemblyFinished>(HandleTestAssemblyFinished)
			&& message.DispatchWhen<_TestAssemblyStarting>(HandleTestAssemblyStarting)
			&& message.DispatchWhen<_TestCaseCleanupFailure>(HandleTestCaseCleanupFailure)
			&& message.DispatchWhen<_TestCaseFinished>(HandleTestCaseFinished)
			&& message.DispatchWhen<_TestCaseStarting>(HandleTestCaseStarting)
			&& message.DispatchWhen<_TestClassCleanupFailure>(HandleTestClassCleanupFailure)
			&& message.DispatchWhen<_TestCleanupFailure>(HandleTestCleanupFailure)
			&& message.DispatchWhen<_TestCollectionCleanupFailure>(HandleTestCollectionCleanupFailure)
			&& message.DispatchWhen<_TestMethodCleanupFailure>(HandleTestMethodCleanupFailure);

		// XML-only handlers
		if (options.AssemblyElement is not null)
			result =
				message.DispatchWhen<_TestClassFinished>(HandleTestClassFinished)
				&& message.DispatchWhen<_TestClassStarting>(HandleTestClassStarting)
				&& message.DispatchWhen<_TestCollectionFinished>(HandleTestCollectionFinished)
				&& message.DispatchWhen<_TestCollectionStarting>(HandleTestCollectionStarting)
				&& message.DispatchWhen<_TestFailed>(HandleTestFailed)
				&& message.DispatchWhen<_TestFinished>(HandleTestFinished)
				&& message.DispatchWhen<_TestMethodFinished>(HandleTestMethodFinished)
				&& message.DispatchWhen<_TestMethodStarting>(HandleTestMethodStarting)
				&& message.DispatchWhen<_TestNotRun>(HandleTestNotRun)
				&& message.DispatchWhen<_TestPassed>(HandleTestPassed)
				&& message.DispatchWhen<_TestSkipped>(HandleTestSkipped)
				&& message.DispatchWhen<_TestStarting>(HandleTestStarting)
				&& result;

		// Do the message conversions last, since they consume values created by the general handlers
		ConvertToRunnerMessageAndDispatch(message);

		// Dispatch to the reporter handler
		result =
			innerSink.OnMessage(message)
			&& result
			&& (options.CancelThunk == null || !options.CancelThunk());

		// Don't request stop until after the inner handler has had a chance to process the message
		// per https://github.com/xunit/visualstudio.xunit/issues/396
		if (stopRequested)
		{
			Finished.Set();
			stopEvent?.Set();
		}

		return result;
	}


	void SendLongRunningMessage()
	{
		if (executingTestCases is null)
			return;

		Dictionary<_ITestCaseMetadata, TimeSpan> longRunningTestCases;
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
				new _DiagnosticMessage(
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
				return;

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

	static string XmlEscape(string? value)
	{
		if (value == null)
			return string.Empty;

		value =
			value
				.Replace("\\", "\\\\")
				.Replace("\r", "\\r")
				.Replace("\n", "\\n")
				.Replace("\t", "\\t")
				.Replace("\0", "\\0")
				.Replace("\a", "\\a")
				.Replace("\b", "\\b")
				.Replace("\v", "\\v")
				.Replace("\"", "\\\"")
				.Replace("\f", "\\f");

		var escapedValue = new StringBuilder(value.Length);
		for (var idx = 0; idx < value.Length; ++idx)
		{
			char ch = value[idx];
			if (ch < 32)
				escapedValue.Append(string.Format(CultureInfo.InvariantCulture, @"\x{0:x2}", +ch));
			else if (char.IsSurrogatePair(value, idx)) // Takes care of the case when idx + 1 == value.Length
			{
				escapedValue.Append(ch); // Append valid surrogate chars like normal
				escapedValue.Append(value[++idx]);
			}
			// Check for invalid chars and append them like \x----
			else if (char.IsSurrogate(ch) || ch == '\uFFFE' || ch == '\uFFFF')
				escapedValue.Append(string.Format(CultureInfo.InvariantCulture, @"\x{0:x4}", +ch));
			else
				escapedValue.Append(ch);
		}

		return escapedValue.ToString();
	}
}
