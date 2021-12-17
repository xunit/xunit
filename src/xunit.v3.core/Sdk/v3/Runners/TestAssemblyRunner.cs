using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// A base class that provides default behavior when running tests in an assembly. It groups the tests
	/// by test collection, and then runs the individual test collections.
	/// </summary>
	/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
	/// derive from <see cref="_ITestCase"/>.</typeparam>
	public abstract class TestAssemblyRunner<TTestCase> : IAsyncDisposable
		where TTestCase : _ITestCase
	{
		CancellationTokenSource cancellationTokenSource = new();
		readonly Lazy<ITestCaseOrderer> defaultTestCaseOrderer;
		readonly Lazy<ITestCollectionOrderer> defaultTestCollectionOrderer;
		readonly _IMessageSink? diagnosticMessageSink;
		_IMessageSink executionMessageSink;
		_ITestFrameworkExecutionOptions executionOptions;
		readonly _IMessageSink? internalDiagnosticMessageSink;
		_ITestAssembly testAssembly;
		IReadOnlyCollection<TTestCase> testCases;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestAssemblyRunner{TTestCase}"/> class.
		/// </summary>
		/// <param name="testAssembly">The assembly that contains the tests to be run.</param>
		/// <param name="testCases">The test cases to be run.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="internalDiagnosticMessageSink">The optional message sink which receives internal <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="executionMessageSink">The message sink to report run status to.</param>
		/// <param name="executionOptions">The user's requested execution options.</param>
		protected TestAssemblyRunner(
			_ITestAssembly testAssembly,
			IReadOnlyCollection<TTestCase> testCases,
			_IMessageSink? diagnosticMessageSink,
			_IMessageSink? internalDiagnosticMessageSink,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			this.testAssembly = Guard.ArgumentNotNull(testAssembly);
			this.testCases = Guard.ArgumentNotNull(testCases);
			this.diagnosticMessageSink = diagnosticMessageSink;
			this.internalDiagnosticMessageSink = internalDiagnosticMessageSink;
			this.executionMessageSink = Guard.ArgumentNotNull(executionMessageSink);
			this.executionOptions = Guard.ArgumentNotNull(executionOptions);

			defaultTestCaseOrderer = new(() => new DefaultTestCaseOrderer());
			defaultTestCollectionOrderer = new(() => new DefaultTestCollectionOrderer());
		}

		/// <summary>
		/// Gets or sets the exception aggregator used to run code and collect exceptions.
		/// </summary>
		protected ExceptionAggregator Aggregator { get; set; } = new();

		/// <summary>
		/// Gets or sets the task cancellation token source, used to cancel the test run.
		/// </summary>
		protected CancellationTokenSource CancellationTokenSource
		{
			get => cancellationTokenSource;
			set => cancellationTokenSource = Guard.ArgumentNotNull(value, nameof(CancellationTokenSource));
		}

		/// <summary>
		/// Gets or sets the user's requested execution options.
		/// </summary>
		protected _ITestFrameworkExecutionOptions ExecutionOptions
		{
			get => executionOptions;
			set => executionOptions = Guard.ArgumentNotNull(value, nameof(ExecutionOptions));
		}

		/// <summary>
		/// Gets or sets the message sink to report run status to.
		/// </summary>
		protected _IMessageSink ExecutionMessageSink
		{
			get => executionMessageSink;
			set => executionMessageSink = Guard.ArgumentNotNull(value, nameof(ExecutionMessageSink));
		}

		/// <summary>
		/// Gets or sets the assembly that contains the tests to be run.
		/// </summary>
		protected _ITestAssembly TestAssembly
		{
			get => testAssembly;
			set => testAssembly = Guard.ArgumentNotNull(value, nameof(TestAssembly));
		}

		/// <summary>
		/// Gets or sets the test cases to be run.
		/// </summary>
		protected IReadOnlyCollection<TTestCase> TestCases
		{
			get => testCases;
			set => testCases = Guard.ArgumentNotNull(value, nameof(TestCases));
		}

		/// <summary>
		/// This method is called just after <see cref="_TestAssemblyStarting"/> is sent, but before any test collections are run.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual ValueTask AfterTestAssemblyStartingAsync() => default;

		/// <summary>
		/// This method is called just before <see cref="_TestAssemblyFinished"/> is sent.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual ValueTask BeforeTestAssemblyFinishedAsync() => default;

		/// <summary>
		/// Creates the message bus to be used for test execution. By default, it inspects
		/// the options for the <see cref="TestOptionsNames.Execution.SynchronousMessageReporting"/>
		/// flag, and if present, creates a message bus that ensures all messages are delivered
		/// on the same thread.
		/// </summary>
		/// <returns>The message bus.</returns>
		protected virtual IMessageBus CreateMessageBus()
		{
			if (ExecutionOptions.SynchronousMessageReportingOrDefault())
				return new SynchronousMessageBus(ExecutionMessageSink);

			return new MessageBus(ExecutionMessageSink, ExecutionOptions.StopOnTestFailOrDefault());
		}

		/// <inheritdoc/>
		public virtual ValueTask DisposeAsync() => default;

		/// <summary>
		/// Override this to provide the target framework against which the assembly was compiled
		/// (f.e., ".NETFramework,Version=v4.7.2"). This value is placed into
		/// <see cref="_TestAssemblyStarting.TargetFramework"/>.
		/// </summary>
		protected virtual string? GetTargetFramework() =>
			TestAssembly.Assembly.GetTargetFramework();

		/// <summary>
		/// Override this to provide a default test case orderer for use when ordering tests in test collections
		/// and test classes. Defaults to an instance of <see cref="DefaultTestCaseOrderer"/>.
		/// </summary>
		protected virtual ITestCaseOrderer GetTestCaseOrderer() =>
			defaultTestCaseOrderer.Value;

		/// <summary>
		/// Orderride this to provide the default test collection order for ordering collections in the assembly.
		/// Defaults to an instance of <see cref="DefaultTestCollectionOrderer"/>.
		/// </summary>
		protected virtual ITestCollectionOrderer GetTestCollectionOrderer() =>
			defaultTestCollectionOrderer.Value;

		/// <summary>
		/// Override this to provide the display name for the test framework (f.e., "xUnit.net 2.0").
		/// This value is placed into <see cref="_TestAssemblyStarting.TestFrameworkDisplayName"/>.
		/// </summary>
		protected abstract string GetTestFrameworkDisplayName();

		/// <summary>
		/// Override this to provide the environment information (f.e., "32-bit .NET 4.0"). This value is
		/// placed into <see cref="_TestAssemblyStarting.TestEnvironment"/>.
		/// </summary>
		protected virtual string GetTestFrameworkEnvironment() => $"{IntPtr.Size * 8}-bit {RuntimeInformation.FrameworkDescription}";

		/// <summary>
		/// Orders the test collections in the assembly.
		/// </summary>
		/// <returns>Test collections in run order (and associated, not-yet-ordered test cases).</returns>
		protected List<Tuple<_ITestCollection, List<TTestCase>>> OrderTestCollections()
		{
			var testCollectionOrderer = GetTestCollectionOrderer();
			var testCasesByCollection =
				TestCases
					.GroupBy(tc => tc.TestCollection, TestCollectionComparer.Instance)
					.ToDictionary(collectionGroup => collectionGroup.Key, collectionGroup => collectionGroup.ToList());

			IReadOnlyCollection<_ITestCollection> orderedTestCollections;

			try
			{
				orderedTestCollections = testCollectionOrderer.OrderTestCollections(testCasesByCollection.Keys);
			}
			catch (Exception ex)
			{
				var innerEx = ex.Unwrap();

				TestContext.Current?.SendDiagnosticMessage(
					"Test collection orderer '{0}' threw '{1}' during ordering: {2}{3}{4}",
					testCollectionOrderer.GetType().FullName,
					innerEx.GetType().FullName,
					innerEx.Message,
					Environment.NewLine,
					innerEx.StackTrace
				);

				orderedTestCollections = testCasesByCollection.Keys.CastOrToReadOnlyCollection();
			}

			return
				orderedTestCollections
					.Select(collection => Tuple.Create(collection, testCasesByCollection[collection]))
					.ToList();
		}

		/// <summary>
		/// Runs the tests in the test assembly.
		/// </summary>
		/// <returns>Returns summary information about the tests that were run.</returns>
		public async ValueTask<RunSummary> RunAsync()
		{
			SetTestContext(TestEngineStatus.Initializing);

			var totalSummary = new RunSummary();
			var currentDirectory = Directory.GetCurrentDirectory();
			var targetFramework = GetTargetFramework();
			var testFrameworkEnvironment = GetTestFrameworkEnvironment();
			var testFrameworkDisplayName = GetTestFrameworkDisplayName();

			using (var messageBus = CreateMessageBus())
			{
				try
				{
					var assemblyFolder = Path.GetDirectoryName(TestAssembly.Assembly.AssemblyPath);
					if (assemblyFolder != null)
						Directory.SetCurrentDirectory(assemblyFolder);
				}
				catch { }

				var testAssemblyStartingMessage = new _TestAssemblyStarting
				{
					AssemblyName = TestAssembly.Assembly.Name,
					AssemblyPath = TestAssembly.Assembly.AssemblyPath,
					AssemblyUniqueID = TestAssembly.UniqueID,
					ConfigFilePath = TestAssembly.ConfigFileName,
					StartTime = DateTimeOffset.Now,
					TargetFramework = targetFramework,
					TestEnvironment = testFrameworkEnvironment,
					TestFrameworkDisplayName = testFrameworkDisplayName,
				};

				if (messageBus.QueueMessage(testAssemblyStartingMessage))
				{
					try
					{
						await AfterTestAssemblyStartingAsync();

						SetTestContext(TestEngineStatus.Running);

						// Want clock time, not aggregated run time
						var clockTimeStopwatch = Stopwatch.StartNew();
						totalSummary = await RunTestCollectionsAsync(messageBus, CancellationTokenSource);
						totalSummary.Time = (decimal)clockTimeStopwatch.Elapsed.TotalSeconds;

						SetTestContext(TestEngineStatus.CleaningUp);

						Aggregator.Clear();
						await BeforeTestAssemblyFinishedAsync();

						if (Aggregator.HasExceptions)
						{
							var cleanupFailure = _TestAssemblyCleanupFailure.FromException(Aggregator.ToException()!, TestAssembly.UniqueID);
							messageBus.QueueMessage(cleanupFailure);
						}
					}
					finally
					{
						var assemblyFinished = new _TestAssemblyFinished
						{
							AssemblyUniqueID = TestAssembly.UniqueID,
							ExecutionTime = totalSummary.Time,
							TestsFailed = totalSummary.Failed,
							TestsRun = totalSummary.Total,
							TestsSkipped = totalSummary.Skipped
						};

						messageBus.QueueMessage(assemblyFinished);

						try
						{
							Directory.SetCurrentDirectory(currentDirectory);
						}
						catch { }
					}
				}
			}

			return totalSummary;
		}

		/// <summary>
		/// Runs the list of test collections. By default, groups the tests by collection and runs them synchronously.
		/// </summary>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		/// <returns>Returns summary information about the tests that were run.</returns>
		protected virtual async ValueTask<RunSummary> RunTestCollectionsAsync(
			IMessageBus messageBus,
			CancellationTokenSource cancellationTokenSource)
		{
			var summary = new RunSummary();

			foreach (var collection in OrderTestCollections())
			{
				summary.Aggregate(await RunTestCollectionAsync(messageBus, collection.Item1, collection.Item2, cancellationTokenSource));
				if (cancellationTokenSource.IsCancellationRequested)
					break;
			}

			return summary;
		}

		/// <summary>
		/// Override this method to run the tests in an individual test collection.
		/// </summary>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="testCollection">The test collection that is being run.</param>
		/// <param name="testCases">The test cases that belong to the test collection.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		/// <returns>Returns summary information about the tests that were run.</returns>
		protected abstract ValueTask<RunSummary> RunTestCollectionAsync(
			IMessageBus messageBus,
			_ITestCollection testCollection,
			IReadOnlyCollection<TTestCase> testCases,
			CancellationTokenSource cancellationTokenSource
		);

		/// <summary>
		/// Sets the current <see cref="TestContext"/> for the current test assembly and the given test assembly status.
		/// </summary>
		/// <param name="testAssemblyStatus">The current test assembly status.</param>
		protected virtual void SetTestContext(TestEngineStatus testAssemblyStatus) =>
			TestContext.SetForTestAssembly(TestAssembly, testAssemblyStatus, CancellationTokenSource.Token, diagnosticMessageSink, internalDiagnosticMessageSink);
	}
}
