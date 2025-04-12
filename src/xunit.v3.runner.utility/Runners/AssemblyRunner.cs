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
	readonly IFrontController controller;
	readonly ManualResetEvent discoveryCompleteEvent = new(true);
	readonly DisposalTracker disposalTracker = new();
	readonly ManualResetEvent executionCompleteEvent = new(true);
	readonly MessageMetadataCache metadataCache = new();
	readonly object statusLock = new();
	int testCasesDiscovered;
	readonly List<ITestCaseDiscovered> testCasesToRun = [];

	static AssemblyRunner()
	{
		MessageTypeNames = [];

		AddMessageTypeName<IDiagnosticMessage>();
		AddMessageTypeName<IDiscoveryComplete>();
		AddMessageTypeName<IErrorMessage>();
		AddMessageTypeName<IInternalDiagnosticMessage>();
		AddMessageTypeName<ITestAssemblyCleanupFailure>();
		AddMessageTypeName<ITestAssemblyFinished>();
		AddMessageTypeName<ITestCaseCleanupFailure>();
		AddMessageTypeName<ITestCaseDiscovered>();
		AddMessageTypeName<ITestClassCleanupFailure>();
		AddMessageTypeName<ITestCleanupFailure>();
		AddMessageTypeName<ITestCollectionCleanupFailure>();
		AddMessageTypeName<ITestFailed>();
		AddMessageTypeName<ITestFinished>();
		AddMessageTypeName<ITestMethodCleanupFailure>();
		AddMessageTypeName<ITestOutput>();
		AddMessageTypeName<ITestPassed>();
		AddMessageTypeName<ITestSkipped>();
		AddMessageTypeName<ITestStarting>();
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
	public AssemblyRunnerStatus Status =>
		!discoveryCompleteEvent.WaitOne(0)
			? AssemblyRunnerStatus.Discovering
			: !executionCompleteEvent.WaitOne(0)
				? AssemblyRunnerStatus.Executing
				: AssemblyRunnerStatus.Idle;

	/// <summary>
	/// Set to be able to filter the test cases to decide which ones to run. If this is not set,
	/// then all test cases will be run.
	/// </summary>
	public Func<ITestCaseDiscovered, bool>? TestCaseFilter { get; set; }

	static void AddMessageTypeName<T>() => MessageTypeNames.Add(typeof(T), typeof(T).FullName!);

	/// <summary>
	/// Call to request that the current run be cancelled. Note that cancellation may not be
	/// instantaneous, and even after cancellation has been acknowledged, you can expect to
	/// receive all the cleanup-related messages.
	/// </summary>
	public void Cancel() =>
		cancelled = true;

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

		await disposalTracker.SafeDisposeAsync();
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

	bool IMessageSink.OnMessage(IMessageSinkMessage message) =>
		OnMessage(message);

	/// <summary>
	/// Reports the presence of a message on the message bus. This method should
	/// never throw exceptions.
	/// </summary>
	/// <param name="message">The message from the message bus</param>
	/// <returns>Return <c>true</c> to continue running tests, or <c>false</c> to stop.</returns>
	protected virtual bool OnMessage(IMessageSinkMessage message)
	{
		// Discovery

		if (DispatchMessage<ITestCaseDiscovered>(message, testDiscovered =>
		{
			++testCasesDiscovered;
			if (TestCaseFilter is null || TestCaseFilter(testDiscovered))
				testCasesToRun.Add(testDiscovered);
		}))
			return !cancelled;

		if (DispatchMessage<IDiscoveryComplete>(message, discoveryComplete =>
		{
			OnDiscoveryComplete?.Invoke(new DiscoveryCompleteInfo(testCasesDiscovered, testCasesToRun.Count));
			discoveryCompleteEvent.Set();
		}))
			return !cancelled;

		// Messages

		if (OnDiagnosticMessage is not null)
			if (DispatchMessage<IDiagnosticMessage>(message, m => OnDiagnosticMessage(new DiagnosticMessageInfo(m.Message))))
				return !cancelled;
		if (OnInternalDiagnosticMessage is not null)
			if (DispatchMessage<IInternalDiagnosticMessage>(message, m => OnInternalDiagnosticMessage(new InternalDiagnosticMessageInfo(m.Message))))
				return !cancelled;

		// Execution

		if (DispatchMessage<ITestAssemblyStarting>(message, metadataCache.Set))
			return !cancelled;
		if (DispatchMessage<ITestAssemblyFinished>(message, assemblyFinished =>
		{
			OnExecutionComplete?.Invoke(new ExecutionCompleteInfo(assemblyFinished.TestsTotal, assemblyFinished.TestsFailed, assemblyFinished.TestsSkipped, assemblyFinished.TestsNotRun, assemblyFinished.ExecutionTime));
			executionCompleteEvent.Set();
			metadataCache.TryRemove(assemblyFinished);
		}))
			return !cancelled;

		if (DispatchMessage<ITestCollectionStarting>(message, metadataCache.Set))
			return !cancelled;
		if (DispatchMessage<ITestCollectionFinished>(message, m => metadataCache.TryRemove(m)))
			return !cancelled;

		if (DispatchMessage<ITestClassStarting>(message, metadataCache.Set))
			return !cancelled;
		if (DispatchMessage<ITestClassFinished>(message, m => metadataCache.TryRemove(m)))
			return !cancelled;

		if (DispatchMessage<ITestMethodStarting>(message, metadataCache.Set))
			return !cancelled;
		if (DispatchMessage<ITestMethodFinished>(message, m => metadataCache.TryRemove(m)))
			return !cancelled;

		if (DispatchMessage<ITestStarting>(message, testStarting =>
		{
			metadataCache.Set(testStarting);
			if (OnTestStarting is not null)
			{
				var (collectionMetadata, classMetadata, methodMetadata, _) = GetTestMetadata(testStarting);
				if (collectionMetadata is not null && classMetadata is not null && methodMetadata is not null)
					OnTestStarting(new(classMetadata.TestClassName, methodMetadata.MethodName, testStarting.Traits, testStarting.TestDisplayName, collectionMetadata.TestCollectionDisplayName));
			}
		}))
			return !cancelled;
		if (DispatchMessage<ITestFinished>(message, testFinished =>
		{
			if (OnTestFinished is not null)
			{
				var (collectionMetadata, classMetadata, methodMetadata, testMetadata) = GetTestMetadata(testFinished);
				if (collectionMetadata is not null && classMetadata is not null && methodMetadata is not null && testMetadata is not null)
					OnTestFinished(new(classMetadata.TestClassName, methodMetadata.MethodName, testMetadata.Traits, testMetadata.TestDisplayName, collectionMetadata.TestCollectionDisplayName, testFinished.ExecutionTime, testFinished.Output));
			}

			metadataCache.TryRemove(testFinished);
		}))
			return !cancelled;

		if (OnTestFailed is not null)
			if (DispatchMessage<ITestFailed>(message, testFailed =>
			{
				var (collectionMetadata, classMetadata, methodMetadata, testMetadata) = GetTestMetadata(testFailed);
				if (collectionMetadata is not null && classMetadata is not null && methodMetadata is not null && testMetadata is not null)
					OnTestFailed(new(classMetadata.TestClassName, methodMetadata.MethodName, testMetadata.Traits, testMetadata.TestDisplayName, collectionMetadata.TestCollectionDisplayName, testFailed.ExecutionTime, testFailed.Output, testFailed.ExceptionTypes[0], testFailed.Messages[0], testFailed.StackTraces[0]));
			}))
				return !cancelled;
		if (OnTestOutput is not null)
			if (DispatchMessage<ITestOutput>(message, testOutput =>
			{
				var (collectionMetadata, classMetadata, methodMetadata, testMetadata) = GetTestMetadata(testOutput);
				if (collectionMetadata is not null && classMetadata is not null && methodMetadata is not null && testMetadata is not null)
					OnTestOutput(new(classMetadata.TestClassName, methodMetadata.MethodName, testMetadata.Traits, testMetadata.TestDisplayName, collectionMetadata.TestCollectionDisplayName, testOutput.Output));
			}))
				return !cancelled;
		if (OnTestPassed is not null)
			if (DispatchMessage<ITestPassed>(message, testPassed =>
			{
				var (collectionMetadata, classMetadata, methodMetadata, testMetadata) = GetTestMetadata(testPassed);
				if (collectionMetadata is not null && classMetadata is not null && methodMetadata is not null && testMetadata is not null)
					OnTestPassed(new(classMetadata.TestClassName, methodMetadata.MethodName, testMetadata.Traits, testMetadata.TestDisplayName, collectionMetadata.TestCollectionDisplayName, testPassed.ExecutionTime, testPassed.Output));
			}))
				return !cancelled;
		if (OnTestSkipped is not null)
			if (DispatchMessage<ITestSkipped>(message, testSkipped =>
			{
				var (collectionMetadata, classMetadata, methodMetadata, testMetadata) = GetTestMetadata(testSkipped);
				if (collectionMetadata is not null && classMetadata is not null && methodMetadata is not null && testMetadata is not null)
					OnTestSkipped(new(classMetadata.TestClassName, methodMetadata.MethodName, testMetadata.Traits, testMetadata.TestDisplayName, collectionMetadata.TestCollectionDisplayName, testSkipped.Reason));
			}))
				return !cancelled;

		if (OnErrorMessage is not null)
		{
			// TODO: This only shows the top level error; should we expand the information available here?
			if (DispatchMessage<IErrorMessage>(message, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.CatastrophicError, m.ExceptionTypes[0], m.Messages[0], m.StackTraces[0]))))
				return !cancelled;
			if (DispatchMessage<ITestAssemblyCleanupFailure>(message, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestAssemblyCleanupFailure, m.ExceptionTypes[0], m.Messages[0], m.StackTraces[0]))))
				return !cancelled;
			if (DispatchMessage<ITestCaseCleanupFailure>(message, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestCaseCleanupFailure, m.ExceptionTypes[0], m.Messages[0], m.StackTraces[0]))))
				return !cancelled;
			if (DispatchMessage<ITestClassCleanupFailure>(message, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestClassCleanupFailure, m.ExceptionTypes[0], m.Messages[0], m.StackTraces[0]))))
				return !cancelled;
			if (DispatchMessage<ITestCleanupFailure>(message, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestCleanupFailure, m.ExceptionTypes[0], m.Messages[0], m.StackTraces[0]))))
				return !cancelled;
			if (DispatchMessage<ITestCollectionCleanupFailure>(message, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestCollectionCleanupFailure, m.ExceptionTypes[0], m.Messages[0], m.StackTraces[0]))))
				return !cancelled;
			if (DispatchMessage<ITestMethodCleanupFailure>(message, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestMethodCleanupFailure, m.ExceptionTypes[0], m.Messages[0], m.StackTraces[0]))))
				return !cancelled;
		}

		return !cancelled;
	}

	(ITestCollectionMetadata?, ITestClassMetadata?, ITestMethodMetadata?, ITestMetadata?) GetTestMetadata(ITestMessage testMessage) =>
		(
			metadataCache.TryGetCollectionMetadata(testMessage),
			metadataCache.TryGetClassMetadata(testMessage),
			metadataCache.TryGetMethodMetadata(testMessage),
			metadataCache.TryGetTestMetadata(testMessage)
		);

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
				filters.AddIncludedClassFilter(typeName);

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
		Guard.ArgumentValid("Cannot set shadowCopyFolder if shadowCopy is false", shadowCopy || shadowCopyFolder is null, nameof(shadowCopyFolder));

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

	static bool DispatchMessage<TMessage>(
		IMessageSinkMessage message,
		Action<TMessage> handler)
			where TMessage : IMessageSinkMessage
	{
		if (message is not TMessage tMessage)
			return false;

		handler(tMessage);
		return true;
	}
}
