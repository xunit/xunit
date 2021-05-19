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
		ExceptionAggregator aggregator = new ExceptionAggregator();
		_IMessageSink diagnosticMessageSink;
		_IMessageSink executionMessageSink;
		_ITestFrameworkExecutionOptions executionOptions;
		_ITestAssembly testAssembly;
		ITestCaseOrderer testCaseOrderer;
		IEnumerable<TTestCase> testCases;
		ITestCollectionOrderer testCollectionOrderer = new DefaultTestCollectionOrderer();

		/// <summary>
		/// Initializes a new instance of the <see cref="TestAssemblyRunner{TTestCase}"/> class.
		/// </summary>
		/// <param name="testAssembly">The assembly that contains the tests to be run.</param>
		/// <param name="testCases">The test cases to be run.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="executionMessageSink">The message sink to report run status to.</param>
		/// <param name="executionOptions">The user's requested execution options.</param>
		protected TestAssemblyRunner(
			_ITestAssembly testAssembly,
			IEnumerable<TTestCase> testCases,
			_IMessageSink diagnosticMessageSink,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			this.testAssembly = Guard.ArgumentNotNull(nameof(testAssembly), testAssembly);
			this.testCases = Guard.ArgumentNotNull(nameof(testCases), testCases);
			this.diagnosticMessageSink = Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);
			this.executionMessageSink = Guard.ArgumentNotNull(nameof(executionMessageSink), executionMessageSink);
			this.executionOptions = Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			testCaseOrderer = new DefaultTestCaseOrderer(DiagnosticMessageSink);
		}

		/// <summary>
		/// Gets or sets the exception aggregator used to run code and collect exceptions.
		/// </summary>
		protected ExceptionAggregator Aggregator
		{
			get => aggregator;
			set => aggregator = Guard.ArgumentNotNull(nameof(Aggregator), value);
		}

		/// <summary>
		/// Gets or sets the user's requested execution options.
		/// </summary>
		protected _ITestFrameworkExecutionOptions ExecutionOptions
		{
			get => executionOptions;
			set => executionOptions = Guard.ArgumentNotNull(nameof(ExecutionOptions), value);
		}

		/// <summary>
		/// Gets or sets the message sink to report diagnostic messages to.
		/// </summary>
		protected _IMessageSink DiagnosticMessageSink
		{
			get => diagnosticMessageSink;
			set => diagnosticMessageSink = Guard.ArgumentNotNull(nameof(DiagnosticMessageSink), value);
		}

		/// <summary>
		/// Gets or sets the message sink to report run status to.
		/// </summary>
		protected _IMessageSink ExecutionMessageSink
		{
			get => executionMessageSink;
			set => executionMessageSink = Guard.ArgumentNotNull(nameof(ExecutionMessageSink), value);
		}

		/// <summary>
		/// Gets or sets the assembly that contains the tests to be run.
		/// </summary>
		protected _ITestAssembly TestAssembly
		{
			get => testAssembly;
			set => testAssembly = Guard.ArgumentNotNull(nameof(TestAssembly), value);
		}

		/// <summary>
		/// Gets or sets the test case orderer that will be used to decide how to order the tests.
		/// </summary>
		protected ITestCaseOrderer TestCaseOrderer
		{
			get => testCaseOrderer;
			set => testCaseOrderer = Guard.ArgumentNotNull(nameof(TestCaseOrderer), value);
		}

		/// <summary>
		/// Gets or sets the test collection orderer that will be used to decide how to order the test collections.
		/// </summary>
		protected ITestCollectionOrderer TestCollectionOrderer
		{
			get => testCollectionOrderer;
			set => testCollectionOrderer = Guard.ArgumentNotNull(nameof(TestCollectionOrderer), value);
		}

		/// <summary>
		/// Gets or sets the test cases to be run.
		/// </summary>
		protected IEnumerable<TTestCase> TestCases
		{
			get => testCases;
			set => testCases = Guard.ArgumentNotNull(nameof(TestCases), value);
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
		/// This method is called just after <see cref="_TestAssemblyStarting"/> is sent, but before any test collections are run.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual Task AfterTestAssemblyStartingAsync() => Task.CompletedTask;

		/// <summary>
		/// This method is called just before <see cref="_TestAssemblyFinished"/> is sent.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual Task BeforeTestAssemblyFinishedAsync() => Task.CompletedTask;

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

		/// <summary>
		/// Orders the test collections using the <see cref="TestCollectionOrderer"/>.
		/// </summary>
		/// <returns>Test collections (and the associated test cases) in run order</returns>
		protected List<Tuple<_ITestCollection, List<TTestCase>>> OrderTestCollections()
		{
			var testCasesByCollection =
				TestCases
					.GroupBy(tc => tc.TestMethod.TestClass.TestCollection, TestCollectionComparer.Instance)
					.ToDictionary(collectionGroup => collectionGroup.Key, collectionGroup => collectionGroup.ToList());

			IEnumerable<_ITestCollection> orderedTestCollections;

			try
			{
				orderedTestCollections = TestCollectionOrderer.OrderTestCollections(testCasesByCollection.Keys);
			}
			catch (Exception ex)
			{
				var innerEx = ex.Unwrap();
				DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Test collection orderer '{TestCollectionOrderer.GetType().FullName}' threw '{innerEx.GetType().FullName}' during ordering: {innerEx.Message}{Environment.NewLine}{innerEx.StackTrace}" });
				orderedTestCollections = testCasesByCollection.Keys.ToList();
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
		public async Task<RunSummary> RunAsync()
		{
			var cancellationTokenSource = new CancellationTokenSource();
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

						var masterStopwatch = Stopwatch.StartNew();
						totalSummary = await RunTestCollectionsAsync(messageBus, cancellationTokenSource);
						// Want clock time, not aggregated run time
						totalSummary.Time = (decimal)masterStopwatch.Elapsed.TotalSeconds;

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
		protected virtual async Task<RunSummary> RunTestCollectionsAsync(
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
		/// <param name="testCases">The test cases to be run.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		/// <returns>Returns summary information about the tests that were run.</returns>
		protected abstract Task<RunSummary> RunTestCollectionAsync(
			IMessageBus messageBus,
			_ITestCollection testCollection,
			IEnumerable<TTestCase> testCases,
			CancellationTokenSource cancellationTokenSource
		);
	}
}
