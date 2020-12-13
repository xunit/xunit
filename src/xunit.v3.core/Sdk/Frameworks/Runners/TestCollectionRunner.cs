using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// A base class that provides default behavior when running tests in a test collection. It groups the tests
	/// by test class, and then runs the individual test classes.
	/// </summary>
	/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
	/// derive from <see cref="_ITestCase"/>.</typeparam>
	public abstract class TestCollectionRunner<TTestCase>
		where TTestCase : class, _ITestCase
	{
		ExceptionAggregator aggregator;
		CancellationTokenSource cancellationTokenSource;
		IMessageBus messageBus;
		ITestCaseOrderer testCaseOrderer;
		IEnumerable<TTestCase> testCases;
		ITestCollection testCollection;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestCollectionRunner{TTestCase}"/> class.
		/// </summary>
		/// <param name="testAssemblyUniqueID">The unique ID of the test assembly that this collection belongs to.</param>
		/// <param name="testCollection">The test collection that contains the tests to be run.</param>
		/// <param name="testCases">The test cases to be run.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the test.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		protected TestCollectionRunner(
			string testAssemblyUniqueID,
			ITestCollection testCollection,
			IEnumerable<TTestCase> testCases,
			IMessageBus messageBus,
			ITestCaseOrderer testCaseOrderer,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
		{
			Guard.ArgumentNotNull(nameof(testAssemblyUniqueID), testAssemblyUniqueID);

			this.testCollection = Guard.ArgumentNotNull(nameof(testCollection), testCollection);
			this.testCases = Guard.ArgumentNotNull(nameof(testCases), testCases);
			this.messageBus = Guard.ArgumentNotNull(nameof(messageBus), messageBus);
			this.testCaseOrderer = Guard.ArgumentNotNull(nameof(testCaseOrderer), testCaseOrderer);
			this.cancellationTokenSource = Guard.ArgumentNotNull(nameof(cancellationTokenSource), cancellationTokenSource);
			this.aggregator = Guard.ArgumentNotNull(nameof(aggregator), aggregator);

			TestAssemblyUniqueID = testAssemblyUniqueID;
			TestCollectionUniqueID = UniqueIDGenerator.ForTestCollection(
				testAssemblyUniqueID,
				testCollection.DisplayName,
				testCollection.CollectionDefinition?.Name
			);
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
		/// Gets or sets the task cancellation token source, used to cancel the test run.
		/// </summary>
		protected CancellationTokenSource CancellationTokenSource
		{
			get => cancellationTokenSource;
			set => cancellationTokenSource = Guard.ArgumentNotNull(nameof(CancellationTokenSource), value);
		}

		/// <summary>
		/// Gets or sets the message bus to report run status to.
		/// </summary>
		protected IMessageBus MessageBus
		{
			get => messageBus;
			set => messageBus = Guard.ArgumentNotNull(nameof(MessageBus), value);
		}

		/// <summary>
		/// Gets the unique ID of the test assembly.
		/// </summary>
		protected string TestAssemblyUniqueID { get; }

		/// <summary>
		/// Gets or sets the test case orderer that will be used to decide how to order the test.
		/// </summary>
		protected ITestCaseOrderer TestCaseOrderer
		{
			get => testCaseOrderer;
			set => testCaseOrderer = Guard.ArgumentNotNull(nameof(TestCaseOrderer), value);
		}

		/// <summary>
		/// Gets or sets the test cases to be run.
		/// </summary>
		protected IEnumerable<TTestCase> TestCases
		{
			get => testCases;
			set => testCases = Guard.ArgumentNotNull(nameof(TestCases), value);
		}

		/// <summary>
		/// Gets or sets the test collection that contains the tests to be run.
		/// </summary>
		protected ITestCollection TestCollection
		{
			get => testCollection;
			set => testCollection = Guard.ArgumentNotNull(nameof(TestCollection), value);
		}

		/// <summary>
		/// Gets the unique ID for the test collection.
		/// </summary>
		protected string TestCollectionUniqueID { get; }

		/// <summary>
		/// This method is called just after <see cref="_TestCollectionStarting"/> is sent, but before any test classes are run.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual Task AfterTestCollectionStartingAsync() => Task.CompletedTask;

		/// <summary>
		/// This method is called just before <see cref="_TestCollectionFinished"/> is sent.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual Task BeforeTestCollectionFinishedAsync() => Task.CompletedTask;

		/// <summary>
		/// Runs the tests in the test collection.
		/// </summary>
		/// <returns>Returns summary information about the tests that were run.</returns>
		public async Task<RunSummary> RunAsync()
		{
			var collectionSummary = new RunSummary();
			var collectionStarting = new _TestCollectionStarting
			{
				AssemblyUniqueID = TestAssemblyUniqueID,
				TestCollectionClass = TestCollection.CollectionDefinition?.Name,
				TestCollectionDisplayName = TestCollection.DisplayName,
				TestCollectionUniqueID = TestCollectionUniqueID
			};

			if (!MessageBus.QueueMessage(collectionStarting))
				CancellationTokenSource.Cancel();
			else
			{
				try
				{
					await AfterTestCollectionStartingAsync();
					collectionSummary = await RunTestClassesAsync();

					Aggregator.Clear();
					await BeforeTestCollectionFinishedAsync();

					if (Aggregator.HasExceptions)
					{
						var collectionCleanupFailure = _TestCollectionCleanupFailure.FromException(Aggregator.ToException()!, TestAssemblyUniqueID, TestCollectionUniqueID);
						if (!MessageBus.QueueMessage(collectionCleanupFailure))
							CancellationTokenSource.Cancel();
					}
				}
				finally
				{
					var collectionFinished = new _TestCollectionFinished
					{
						AssemblyUniqueID = TestAssemblyUniqueID,
						ExecutionTime = collectionSummary.Time,
						TestCollectionUniqueID = TestCollectionUniqueID,
						TestsFailed = collectionSummary.Failed,
						TestsRun = collectionSummary.Total,
						TestsSkipped = collectionSummary.Skipped
					};

					if (!MessageBus.QueueMessage(collectionFinished))
						CancellationTokenSource.Cancel();
				}
			}

			return collectionSummary;
		}

		/// <summary>
		/// Runs the list of test classes. By default, groups the tests by class and runs them synchronously.
		/// </summary>
		/// <returns>Returns summary information about the tests that were run.</returns>
		protected virtual async Task<RunSummary> RunTestClassesAsync()
		{
			var summary = new RunSummary();

			foreach (var testCasesByClass in TestCases.GroupBy(tc => tc.TestMethod.TestClass, TestClassComparer.Instance))
			{
				summary.Aggregate(await RunTestClassAsync(testCasesByClass.Key, (IReflectionTypeInfo)testCasesByClass.Key.Class, testCasesByClass));
				if (CancellationTokenSource.IsCancellationRequested)
					break;
			}

			return summary;
		}

		/// <summary>
		/// Override this method to run the tests in an individual test class.
		/// </summary>
		/// <param name="testClass">The test class to be run.</param>
		/// <param name="class">The CLR class that contains the tests to be run.</param>
		/// <param name="testCases">The test cases to be run.</param>
		/// <returns>Returns summary information about the tests that were run.</returns>
		protected abstract Task<RunSummary> RunTestClassAsync(
			_ITestClass testClass,
			IReflectionTypeInfo @class,
			IEnumerable<TTestCase> testCases
		);
	}
}
