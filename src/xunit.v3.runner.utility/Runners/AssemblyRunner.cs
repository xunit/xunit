using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runners
{
	/// <summary>
	/// A class which makes it simpler for casual runner authors to find and run tests and get results.
	/// </summary>
	public class AssemblyRunner : IAsyncDisposable, _IMessageSink
	{
		static readonly Dictionary<Type, string> MessageTypeNames;

		volatile bool cancelled;
		bool disposed;
		readonly TestAssemblyConfiguration configuration = new();
		readonly IFrontController controller;
		readonly ManualResetEvent discoveryCompleteEvent = new ManualResetEvent(true);
		readonly DisposalTracker disposalTracker = new DisposalTracker();
		readonly ManualResetEvent executionCompleteEvent = new ManualResetEvent(true);
		readonly object statusLock = new object();
		int testCasesDiscovered;
		readonly List<_TestCaseDiscovered> testCasesToRun = new List<_TestCaseDiscovered>();

		static AssemblyRunner()
		{
			MessageTypeNames = new Dictionary<Type, string>();

			AddMessageTypeName<_DiagnosticMessage>();
			AddMessageTypeName<_DiscoveryComplete>();
			AddMessageTypeName<_ErrorMessage>();
			AddMessageTypeName<_TestAssemblyCleanupFailure>();
			AddMessageTypeName<_TestAssemblyFinished>();
			AddMessageTypeName<_TestCaseCleanupFailure>();
			AddMessageTypeName<_TestCaseDiscovered>();
			AddMessageTypeName<_TestClassCleanupFailure>();
			AddMessageTypeName<_TestCleanupFailure>();
			AddMessageTypeName<_TestCollectionCleanupFailure>();
			AddMessageTypeName<_TestFailed>();
			AddMessageTypeName<_TestFinished>();
			AddMessageTypeName<_TestMethodCleanupFailure>();
			AddMessageTypeName<_TestOutput>();
			AddMessageTypeName<_TestPassed>();
			AddMessageTypeName<_TestSkipped>();
			AddMessageTypeName<_TestStarting>();
		}

		AssemblyRunner(
			AppDomainSupport appDomainSupport,
			string assemblyFileName,
			string? configFileName = null,
			bool shadowCopy = true,
			string? shadowCopyFolder = null)
		{
			var project = new XunitProject();
			var projectAssembly = new XunitProjectAssembly(project)
			{
				AssemblyFilename = assemblyFileName,
				ConfigFilename = configFileName,
			};

			ConfigReader.Load(projectAssembly.Configuration, projectAssembly.AssemblyFilename, projectAssembly.ConfigFilename);
			projectAssembly.Configuration.AppDomain = appDomainSupport;
			projectAssembly.Configuration.ShadowCopy = shadowCopy;
			projectAssembly.Configuration.ShadowCopyFolder = shadowCopyFolder;

			project.Add(projectAssembly);

			controller = XunitFrontController.ForDiscoveryAndExecution(projectAssembly, diagnosticMessageSink: this);
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
		public Func<_TestCaseDiscovered, bool>? TestCaseFilter { get; set; }

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
					throw new ObjectDisposedException(GetType().FullName);

				if (Status != AssemblyRunnerStatus.Idle)
					throw new InvalidOperationException("Cannot dispose the assembly runner when it's not idle");

				disposed = true;
			}

			await disposalTracker.DisposeAsync();
			discoveryCompleteEvent?.Dispose();
			executionCompleteEvent?.Dispose();
		}

		_ITestFrameworkDiscoveryOptions GetDiscoveryOptions(
			bool? diagnosticMessages,
			TestMethodDisplay? methodDisplay,
			TestMethodDisplayOptions? methodDisplayOptions,
			bool? preEnumerateTheories,
			bool? internalDiagnosticMessages)
		{
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(configuration);
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

		_ITestFrameworkExecutionOptions GetExecutionOptions(
			bool? diagnosticMessages,
			bool? parallel,
			int? maxParallelThreads,
			bool? internalDiagnosticMessages)
		{
			var executionOptions = _TestFrameworkOptions.ForExecution(configuration);
			executionOptions.SetSynchronousMessageReporting(true);

			if (diagnosticMessages.HasValue)
				executionOptions.SetDiagnosticMessages(diagnosticMessages);
			if (internalDiagnosticMessages.HasValue)
				executionOptions.SetDiagnosticMessages(internalDiagnosticMessages);
			if (parallel.HasValue)
				executionOptions.SetDisableParallelization(!parallel.GetValueOrDefault());
			if (maxParallelThreads.HasValue)
				executionOptions.SetMaxParallelThreads(maxParallelThreads);

			return executionOptions;
		}

		/// <summary>
		/// Starts running tests from a single type (if provided) or the whole assembly (if not). This call returns
		/// immediately, and status results are dispatched to the Info>s on this class. Callers can check <see cref="Status"/>
		/// to find out the current status.
		/// </summary>
		/// <param name="typeName">The (optional) type name of the single test class to run</param>
		/// <param name="diagnosticMessages">Set to <c>true</c> to enable diagnostic messages; set to <c>false</c> to disable them.
		/// By default, uses the value from the assembly configuration file.</param>
		/// <param name="methodDisplay">Set to choose the default display name style for test methods.
		/// By default, uses the value from the assembly configuration file. (This parameter is ignored for xUnit.net v1 tests.)</param>
		/// <param name="methodDisplayOptions">Set to choose the default display name style options for test methods.
		/// By default, uses the value from the assembly configuration file. (This parameter is ignored for xUnit.net v1 tests.)</param>
		/// <param name="preEnumerateTheories">Set to <c>true</c> to pre-enumerate individual theory tests; set to <c>false</c> to use
		/// a single test case for the theory. By default, uses the value from the assembly configuration file. (This parameter is ignored
		/// for xUnit.net v1 tests.)</param>
		/// <param name="parallel">Set to <c>true</c> to run test collections in parallel; set to <c>false</c> to run them sequentially.
		/// By default, uses the value from the assembly configuration file. (This parameter is ignored for xUnit.net v1 tests.)</param>
		/// <param name="maxParallelThreads">Set to 0 to use unlimited threads; set to any other positive integer to limit to an exact number
		/// of threads. By default, uses the value from the assembly configuration file. (This parameter is ignored for xUnit.net v1 tests.)</param>
		/// <param name="internalDiagnosticMessages">Set to <c>true</c> to enable internal diagnostic messages; set to <c>false</c> to disable them.
		/// By default, uses the value from the assembly configuration file.</param>
		public void Start(
			string? typeName = null,
			bool? diagnosticMessages = null,
			TestMethodDisplay? methodDisplay = null,
			TestMethodDisplayOptions? methodDisplayOptions = null,
			bool? preEnumerateTheories = null,
			bool? parallel = null,
			int? maxParallelThreads = null,
			bool? internalDiagnosticMessages = null)
		{
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
				var discoveryOptions = GetDiscoveryOptions(diagnosticMessages, methodDisplay, methodDisplayOptions, preEnumerateTheories, internalDiagnosticMessages);
				var filters = new XunitFilters();
				if (typeName != null)
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

				var executionOptions = GetExecutionOptions(diagnosticMessages, parallel, maxParallelThreads, internalDiagnosticMessages);
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
			Guard.FileExists(nameof(assemblyFileName), assemblyFileName);
			Guard.ArgumentValid(nameof(shadowCopyFolder), "Cannot set shadowCopyFolder if shadowCopy is false", shadowCopy == true || shadowCopyFolder == null);

			return new AssemblyRunner(AppDomainSupport.Required, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);
		}
#endif

		/// <summary>
		/// Creates an assembly runner that discovers and runs tests without a separate app domain.
		/// </summary>
		/// <param name="assemblyFileName">The test assembly.</param>
		public static AssemblyRunner WithoutAppDomain(string assemblyFileName)
		{
			Guard.FileExists(nameof(assemblyFileName), assemblyFileName);

			return new AssemblyRunner(AppDomainSupport.Denied, assemblyFileName);
		}

		bool DispatchMessage<TMessage>(_MessageSinkMessage message, HashSet<string>? messageTypes, Action<TMessage> handler)
			where TMessage : _MessageSinkMessage
		{
			if (messageTypes == null || !MessageTypeNames.TryGetValue(typeof(TMessage), out var typeName) || !messageTypes.Contains(typeName))
				return false;

			handler((TMessage)message);
			return true;
		}

		bool _IMessageSink.OnMessage(_MessageSinkMessage message)
		{
			// Temporary
			var messageTypes = default(HashSet<string>);

			if (DispatchMessage<_TestCaseDiscovered>(message, messageTypes, testDiscovered =>
			{
				++testCasesDiscovered;
				if (TestCaseFilter == null || TestCaseFilter(testDiscovered))
					testCasesToRun.Add(testDiscovered);
			}))
				return !cancelled;

			if (DispatchMessage<_DiscoveryComplete>(message, messageTypes, discoveryComplete =>
			{
				OnDiscoveryComplete?.Invoke(new DiscoveryCompleteInfo(testCasesDiscovered, testCasesToRun.Count));
				discoveryCompleteEvent.Set();
			}))
				return !cancelled;

			if (DispatchMessage<_TestAssemblyFinished>(message, messageTypes, assemblyFinished =>
			{
				OnExecutionComplete?.Invoke(new ExecutionCompleteInfo(assemblyFinished.TestsRun, assemblyFinished.TestsFailed, assemblyFinished.TestsSkipped, assemblyFinished.ExecutionTime));
				executionCompleteEvent.Set();
			}))
				return !cancelled;

			if (OnDiagnosticMessage != null)
				if (DispatchMessage<_DiagnosticMessage>(message, messageTypes, m => OnDiagnosticMessage(new DiagnosticMessageInfo(m.Message))))
					return !cancelled;
#if false  // TODO: No simple conversions here yet
			if (OnTestFailed != null)
				if (DispatchMessage<_TestFailed>(message, messageTypes, m => OnTestFailed(new TestFailedInfo(m.TestClass.Class.Name, m.TestMethod.Method.Name, m.TestCase.Traits, m.Test.DisplayName, m.TestCollection.DisplayName, m.ExecutionTime, m.Output, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
					return !cancelled;
			if (OnTestFinished != null)
				if (DispatchMessage<_TestFinished>(message, messageTypes, m => OnTestFinished(new TestFinishedInfo(m.TestClass.Class.Name, m.TestMethod.Method.Name, m.TestCase.Traits, m.Test.DisplayName, m.TestCollection.DisplayName, m.ExecutionTime, m.Output))))
					return !cancelled;
			if (OnTestOutput != null)
				if (DispatchMessage<_TestOutput>(message, messageTypes, m => OnTestOutput(new TestOutputInfo(m.TestClass.Class.Name, m.TestMethod.Method.Name, m.TestCase.Traits, m.Test.DisplayName, m.TestCollection.DisplayName, m.Output))))
					return !cancelled;
			if (OnTestPassed != null)
				if (DispatchMessage<_TestPassed>(message, messageTypes, m => OnTestPassed(new TestPassedInfo(m.TestClass.Class.Name, m.TestMethod.Method.Name, m.TestCase.Traits, m.Test.DisplayName, m.TestCollection.DisplayName, m.ExecutionTime, m.Output))))
					return !cancelled;
			if (OnTestSkipped != null)
				if (DispatchMessage<_TestSkipped>(message, messageTypes, m => OnTestSkipped(new TestSkippedInfo(m.TestClass.Class.Name, m.TestMethod.Method.Name, m.TestCase.Traits, m.Test.DisplayName, m.TestCollection.DisplayName, m.Reason))))
					return !cancelled;
			if (OnTestStarting != null)
				if (DispatchMessage<_TestStarting>(message, messageTypes, m => OnTestStarting(new TestStartingInfo(m.TestClass.Class.Name, m.TestMethod.Method.Name, m.TestCase.Traits, m.Test.DisplayName, m.TestCollection.DisplayName))))
					return !cancelled;
#endif

			if (OnErrorMessage != null)
			{
				if (DispatchMessage<_ErrorMessage>(message, messageTypes, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.CatastrophicError, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
					return !cancelled;
				if (DispatchMessage<_TestAssemblyCleanupFailure>(message, messageTypes, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestAssemblyCleanupFailure, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
					return !cancelled;
				if (DispatchMessage<_TestCaseCleanupFailure>(message, messageTypes, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestCaseCleanupFailure, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
					return !cancelled;
				if (DispatchMessage<_TestClassCleanupFailure>(message, messageTypes, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestClassCleanupFailure, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
					return !cancelled;
				if (DispatchMessage<_TestCleanupFailure>(message, messageTypes, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestCleanupFailure, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
					return !cancelled;
				if (DispatchMessage<_TestCollectionCleanupFailure>(message, messageTypes, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestCollectionCleanupFailure, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
					return !cancelled;
				if (DispatchMessage<_TestMethodCleanupFailure>(message, messageTypes, m => OnErrorMessage(new ErrorMessageInfo(ErrorMessageType.TestMethodCleanupFailure, m.ExceptionTypes.FirstOrDefault(), m.Messages.FirstOrDefault(), m.StackTraces.FirstOrDefault()))))
					return !cancelled;
			}

			return !cancelled;
		}
	}
}
