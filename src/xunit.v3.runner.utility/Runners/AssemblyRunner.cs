using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runners;

/// <summary>
/// A class which makes it simpler for casual runner authors to find and run tests and get results.
/// </summary>
public class AssemblyRunner : IAsyncDisposable, IMessageSink
{
	static readonly Dictionary<Type, string> MessageTypeNames;

	volatile bool cancelled;
	bool disposed;
	readonly TestAssemblyConfiguration configuration = new();
#pragma warning disable CA2213 // This is disposed by DisposalTracker
	readonly IFrontController controller;
#pragma warning restore CA2213
	readonly ManualResetEvent discoveryCompleteEvent = new(true);
	readonly DisposalTracker disposalTracker = new();
	readonly ManualResetEvent executionCompleteEvent = new(true);
	readonly object statusLock = new();
	int testCasesDiscovered;
	readonly List<TestCaseDiscovered> testCasesToRun = new();

	static AssemblyRunner()
	{
		MessageTypeNames = [];

		AddMessageTypeName<DiagnosticMessage>();
		AddMessageTypeName<DiscoveryComplete>();
		AddMessageTypeName<ErrorMessage>();
		AddMessageTypeName<InternalDiagnosticMessage>();
		AddMessageTypeName<TestAssemblyCleanupFailure>();
		AddMessageTypeName<TestAssemblyFinished>();
		AddMessageTypeName<TestCaseCleanupFailure>();
		AddMessageTypeName<TestCaseDiscovered>();
		AddMessageTypeName<TestClassCleanupFailure>();
		AddMessageTypeName<TestCleanupFailure>();
		AddMessageTypeName<TestCollectionCleanupFailure>();
		AddMessageTypeName<TestFailed>();
		AddMessageTypeName<TestFinished>();
		AddMessageTypeName<TestMethodCleanupFailure>();
		AddMessageTypeName<TestOutput>();
		AddMessageTypeName<TestPassed>();
		AddMessageTypeName<TestSkipped>();
		AddMessageTypeName<TestStarting>();
	}

	AssemblyRunner(
		AppDomainSupport appDomainSupport,
		string assemblyFileName,
		string? configFileName = null,
		bool shadowCopy = true,
		string? shadowCopyFolder = null)
	{
		Guard.ArgumentNotNullOrEmpty(assemblyFileName);
		Guard.FileExists(assemblyFileName);

		var metadata =
			AssemblyUtility.GetAssemblyMetadata(assemblyFileName)
				?? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Assembly '{0}' is not a valid .NET assembly", assemblyFileName), nameof(assemblyFileName));

		var project = new XunitProject();
		var projectAssembly = new XunitProjectAssembly(project, assemblyFileName, metadata) { ConfigFileName = configFileName };

		ConfigReader.Load(projectAssembly.Configuration, projectAssembly.AssemblyFileName, projectAssembly.ConfigFileName);
		projectAssembly.Configuration.AppDomain = appDomainSupport;
		projectAssembly.Configuration.ShadowCopy = shadowCopy;
		projectAssembly.Configuration.ShadowCopyFolder = shadowCopyFolder;

		project.Add(projectAssembly);

		controller =
			XunitFrontController.Create(projectAssembly, diagnosticMessageSink: this)
				?? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Assembly '{0}' does not appear to be an xUnit.net test assembly", assemblyFileName), nameof(assemblyFileName));

		disposalTracker.Add(controller);

		ConfigReader.Load(configuration, assemblyFileName, configFileName);
	}

	/// <summary>
	/// Set to get notification of diagnostic messages.
	/// </summary>
	public Action<DiagnosticMessageInfo>? OnDiagnosticMessage { get; set; }

	/// <summary>
	/// Set to get notification of when test discovery is complete.
	/// </summary>
	public Action<DiscoveryCompleteInfo>? OnDiscoveryComplete { get; set; }

	/// <summary>
	/// Set to get notification of error messages (unhandled exceptions outside of tests).
	/// </summary>
	public Action<ErrorMessageInfo>? OnErrorMessage { get; set; }

	/// <summary>
	/// Set to get notification of when test execution is complete.
	/// </summary>
	public Action<ExecutionCompleteInfo>? OnExecutionComplete { get; set; }

	/// <summary>
	/// Set to get notification of internal diagnostic messages.
	/// </summary>
	public Action<InternalDiagnosticMessageInfo>? OnInternalDiagnosticMessage { get; set; }

	/// <summary>
	/// Set to get notification of failed tests.
	/// </summary>
	public Action<TestFailedInfo>? OnTestFailed { get; set; }

	/// <summary>
	/// Set to get notification of finished tests (regardless of outcome).
	/// </summary>
	public Action<TestFinishedInfo>? OnTestFinished { get; set; }

	/// <summary>
	/// Set to get real-time notification of test output (for xUnit.net v2 tests only).
	/// Note that output is captured and reported back to all the test completion Info>s
	/// in addition to being sent to this Info>.
	/// </summary>
	public Action<TestOutputInfo>? OnTestOutput { get; set; }

	/// <summary>
	/// Set to get notification of passing tests.
	/// </summary>
	public Action<TestPassedInfo>? OnTestPassed { get; set; }

	/// <summary>
	/// Set to get notification of skipped tests.
	/// </summary>
	public Action<TestSkippedInfo>? OnTestSkipped { get; set; }

	/// <summary>
	/// Set to get notification of when tests start running.
	/// </summary>
	public Action<TestStartingInfo>? OnTestStarting { get; set; }

	/// <summary>
	/// Gets the current status of the assembly runner
	/// </summary>
	public AssemblyRunnerStatus Status
	{
		get
		{
			if (!discoveryCompleteEvent.WaitOne(0))
				return AssemblyRunnerStatus.Discovering;
			if (!executionCompleteEvent.WaitOne(0))
				return AssemblyRunnerStatus.Executing;

			return AssemblyRunnerStatus.Idle;
		}
	}

	/// <summary>
	/// Set to be able to filter the test cases to decide which ones to run. If this is not set,
	/// then all test cases will be run.
	/// </summary>
	public Func<TestCaseDiscovered, bool>? TestCaseFilter { get; set; }

	static void AddMessageTypeName<T>() => MessageTypeNames.Add(typeof(T), typeof(T).FullName!);

	/// <summary>
	/// Call to request that the current run be cancelled. Note that cancellation may not be
	/// instantaneous, and even after cancellation has been acknowledged, you can expect to
	/// receive all the cleanup-related messages.
	/// </summary>
	public void Cancel()
	{
		cancelled = true;
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		lock (statusLock)
		{
			if (disposed)
				return;

			if (Status != AssemblyRunnerStatus.Idle)
				throw new InvalidOperationException("Cannot dispose the assembly runner when it's not idle");

			disposed = true;
		}

		GC.SuppressFinalize(this);

		await disposalTracker.DisposeAsync();
		discoveryCompleteEvent?.Dispose();
		executionCompleteEvent?.Dispose();
	}

	ITestFrameworkDiscoveryOptions GetDiscoveryOptions(
		bool? diagnosticMessages,
		bool? internalDiagnosticMessages,
		TestMethodDisplay? methodDisplay,
		TestMethodDisplayOptions? methodDisplayOptions,
		bool? preEnumerateTheories)
	{
		var discoveryOptions = TestFrameworkOptions.ForDiscovery(configuration);
		discoveryOptions.SetSynchronousMessageReporting(true);

		if (diagnosticMessages.HasValue)
			discoveryOptions.SetDiagnosticMessages(diagnosticMessages);
		if (internalDiagnosticMessages.HasValue)
			discoveryOptions.SetDiagnosticMessages(internalDiagnosticMessages);
		if (methodDisplay.HasValue)
			discoveryOptions.SetMethodDisplay(methodDisplay);
		if (methodDisplayOptions.HasValue)
			discoveryOptions.SetMethodDisplayOptions(methodDisplayOptions);
		if (preEnumerateTheories.HasValue)
			discoveryOptions.SetPreEnumerateTheories(preEnumerateTheories);

		return discoveryOptions;
	}

	ITestFrameworkExecutionOptions GetExecutionOptions(
		bool? diagnosticMessages,
		bool? internalDiagnosticMessages,
		int? maxParallelThreads,
		bool? parallel,
		ParallelAlgorithm? parallelAlgorithm)
	{
		var executionOptions = TestFrameworkOptions.ForExecution(configuration);
		executionOptions.SetSynchronousMessageReporting(true);

		if (diagnosticMessages.HasValue)
			executionOptions.SetDiagnosticMessages(diagnosticMessages);
		if (internalDiagnosticMessages.HasValue)
			executionOptions.SetDiagnosticMessages(internalDiagnosticMessages);
		if (parallel.HasValue)
			executionOptions.SetDisableParallelization(!parallel.GetValueOrDefault());
		if (maxParallelThreads.HasValue)
			executionOptions.SetMaxParallelThreads(maxParallelThreads);
		if (parallelAlgorithm.HasValue)
			executionOptions.SetParallelAlgorithm(parallelAlgorithm);

		return executionOptions;
	}

	bool IMessageSink.OnMessage(MessageSinkMessage message) =>
		OnMessage(message);

	/// <summary>
	/// Reports the presence of a message on the message bus. This method should
	/// never throw exceptions.
	/// </summary>
	/// <param name="message">The message from the message bus</param>
	/// <returns>Return <c>true</c> to continue running tests, or <c>false</c> to stop.</returns>
	protected virtual bool OnMessage(MessageSinkMessage message)
	{
		// Temporary
		var messageTypes = default(HashSet<string>);

		if (DispatchMessage<TestCaseDiscovered>(message, messageTypes, testDiscovered =>
		{
			++testCasesDiscovered;
			if (TestCaseFilter is null || TestCaseFilter(testDiscovered))
				testCasesToRun.Add(testDiscovered);
		}))
			return !cancelled;

		if (DispatchMessage<DiscoveryComplete>(message, messageTypes, discoveryComplete =>
		{
			OnDiscoveryComplete?.Invoke(new DiscoveryCompleteInfo(testCasesDiscovered, testCasesToRun.Count));
			discoveryCompleteEvent.Set();
		}))
			return !cancelled;

		if (DispatchMessage<TestAssemblyFinished>(message, messageTypes, assemblyFinished =>
		{
			OnExecutionComplete?.Invoke(new ExecutionCompleteInfo(assemblyFinished.TestsTotal, assemblyFinished.TestsFailed, assemblyFinished.TestsSkipped, assemblyFinished.TestsNotRun, assemblyFinished.ExecutionTime));
			executionCompleteEvent.Set();
		}))
			return !cancelled;

		if (OnDiagnosticMessage is not null)
			if (DispatchMessage<DiagnosticMessage>(message, messageTypes, m => OnDiagnosticMessage(new DiagnosticMessageInfo(m.Message))))
				return !cancelled;
		if (OnInternalDiagnosticMessage is not null)
			if (DispatchMessage<InternalDiagnosticMessage>(message, messageTypes, m => OnInternalDiagnosticMessage(new InternalDiagnosticMessageInfo(m.Message))))
				return !cancelled;
#if false  // TODO: No simple conversions here yet
		if (OnTestFailed is not null)
			if (DispatchMessage<_TestFailed>(message, messageTypes, m => OnTestFailed(new TestFailedInfo(m.TestClass.Class.Name, m.TestMethod.Method.Name, m.TestCase.Traits, m.Test.DisplayName, m.TestCollection.DisplayName, m.ExecutionTime, m.Output, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
				return !cancelled;
		if (OnTestFinished is not null)
			if (DispatchMessage<_TestFinished>(message, messageTypes, m => OnTestFinished(new TestFinishedInfo(m.TestClass.Class.Name, m.TestMethod.Method.Name, m.TestCase.Traits, m.Test.DisplayName, m.TestCollection.DisplayName, m.ExecutionTime, m.Output))))
				return !cancelled;
		if (OnTestOutput is not null)
			if (DispatchMessage<_TestOutput>(message, messageTypes, m => OnTestOutput(new TestOutputInfo(m.TestClass.Class.Name, m.TestMethod.Method.Name, m.TestCase.Traits, m.Test.DisplayName, m.TestCollection.DisplayName, m.Output))))
				return !cancelled;
		if (OnTestPassed is not null)
			if (DispatchMessage<_TestPassed>(message, messageTypes, m => OnTestPassed(new TestPassedInfo(m.TestClass.Class.Name, m.TestMethod.Method.Name, m.TestCase.Traits, m.Test.DisplayName, m.TestCollection.DisplayName, m.ExecutionTime, m.Output))))
				return !cancelled;
		if (OnTestSkipped is not null)
			if (DispatchMessage<_TestSkipped>(message, messageTypes, m => OnTestSkipped(new TestSkippedInfo(m.TestClass.Class.Name, m.TestMethod.Method.Name, m.TestCase.Traits, m.Test.DisplayName, m.TestCollection.DisplayName, m.Reason))))
				return !cancelled;
		if (OnTestStarting is not null)
			if (DispatchMessage<_TestStarting>(message, messageTypes, m => OnTestStarting(new TestStartingInfo(m.TestClass.Class.Name, m.TestMethod.Method.Name, m.TestCase.Traits, m.Test.DisplayName, m.TestCollection.DisplayName))))
				return !cancelled;
#endif

		if (OnErrorMessage is not null)
		{
			if (DispatchMessage<ErrorMessage>(message, messageTypes, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.CatastrophicError, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
				return !cancelled;
			if (DispatchMessage<TestAssemblyCleanupFailure>(message, messageTypes, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestAssemblyCleanupFailure, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
				return !cancelled;
			if (DispatchMessage<TestCaseCleanupFailure>(message, messageTypes, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestCaseCleanupFailure, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
				return !cancelled;
			if (DispatchMessage<TestClassCleanupFailure>(message, messageTypes, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestClassCleanupFailure, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
				return !cancelled;
			if (DispatchMessage<TestCleanupFailure>(message, messageTypes, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestCleanupFailure, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
				return !cancelled;
			if (DispatchMessage<TestCollectionCleanupFailure>(message, messageTypes, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestCollectionCleanupFailure, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
				return !cancelled;
			if (DispatchMessage<TestMethodCleanupFailure>(message, messageTypes, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestMethodCleanupFailure, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
				return !cancelled;
		}

		return !cancelled;
	}

	/// <summary>
	/// Starts running tests. This call returns immediately, and status results are dispatched to the events on this class.
	/// Callers can check <see cref="Status"/> to find out the current status.
	/// </summary>
	/// <param name="startOptions">The optional start options.</param>
	public void Start(AssemblyRunnerStartOptions? startOptions = null)
	{
		startOptions ??= AssemblyRunnerStartOptions.Empty;

		lock (statusLock)
		{
			if (Status != AssemblyRunnerStatus.Idle)
				throw new InvalidOperationException("Calling Start is not valid when the current status is not idle.");

			cancelled = false;
			testCasesDiscovered = 0;
			testCasesToRun.Clear();
			discoveryCompleteEvent.Reset();
			executionCompleteEvent.Reset();
		}

		ThreadPool.QueueUserWorkItem(_ =>
		{
			// TODO: This should be restructured to use FindAndRun, which will require a new design for AssemblyRunner
			var discoveryOptions = GetDiscoveryOptions(startOptions.DiagnosticMessages, startOptions.InternalDiagnosticMessages, startOptions.MethodDisplay, startOptions.MethodDisplayOptions, startOptions.PreEnumerateTheories);
			var filters = new XunitFilters();
			foreach (var typeName in startOptions.TypesToRun)
				filters.IncludedClasses.Add(typeName);

			var findSettings = new FrontControllerFindSettings(discoveryOptions, filters);
			controller.Find(this, findSettings);

			discoveryCompleteEvent.WaitOne();
			if (cancelled)
			{
				// Synthesize the execution complete message, since we're not going to run at all
				OnExecutionComplete?.Invoke(ExecutionCompleteInfo.Empty);
				return;
			}

			var executionOptions = GetExecutionOptions(startOptions.DiagnosticMessages, startOptions.InternalDiagnosticMessages, startOptions.MaxParallelThreads, startOptions.Parallel, startOptions.ParallelAlgorithm);
			var runSettings = new FrontControllerRunSettings(executionOptions, testCasesToRun.Select(tc => tc.Serialization).CastOrToReadOnlyCollection());
			controller.Run(this, runSettings);
			executionCompleteEvent.WaitOne();
		});
	}

#if NETFRAMEWORK
	/// <summary>
	/// Creates an assembly runner that discovers and run tests in a separate app domain.
	/// </summary>
	/// <param name="assemblyFileName">The test assembly.</param>
	/// <param name="configFileName">The test assembly configuration file.</param>
	/// <param name="shadowCopy">If set to <c>true</c>, runs tests in a shadow copied app domain, which allows
	/// tests to be discovered and run without locking assembly files on disk.</param>
	/// <param name="shadowCopyFolder">The path on disk to use for shadow copying; if <c>null</c>, a folder
	/// will be automatically (randomly) generated</param>
	public static AssemblyRunner WithAppDomain(
		string assemblyFileName,
		string? configFileName = null,
		bool shadowCopy = true,
		string? shadowCopyFolder = null)
	{
		Guard.FileExists(assemblyFileName);
		Guard.ArgumentValid("Cannot set shadowCopyFolder if shadowCopy is false", shadowCopy == true || shadowCopyFolder is null, nameof(shadowCopyFolder));

		return new AssemblyRunner(AppDomainSupport.Required, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);
	}
#endif

	/// <summary>
	/// Creates an assembly runner that discovers and runs tests without a separate app domain.
	/// </summary>
	/// <param name="assemblyFileName">The test assembly.</param>
	public static AssemblyRunner WithoutAppDomain(string assemblyFileName)
	{
		Guard.FileExists(assemblyFileName);

		return new AssemblyRunner(AppDomainSupport.Denied, assemblyFileName);
	}

	static bool DispatchMessage<TMessage>(MessageSinkMessage message, HashSet<string>? messageTypes, Action<TMessage> handler)
		where TMessage : MessageSinkMessage
	{
		if (messageTypes is null || !MessageTypeNames.TryGetValue(typeof(TMessage), out var typeName) || !messageTypes.Contains(typeName))
			return false;

		handler((TMessage)message);
		return true;
	}
}
