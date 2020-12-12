using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// A base class that provides default behavior when running test cases.
	/// </summary>
	/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
	/// derive from <see cref="_ITestCase"/>.</typeparam>
	public abstract class TestCaseRunner<TTestCase>
		where TTestCase : class, _ITestCase
	{
		ExceptionAggregator aggregator;
		CancellationTokenSource cancellationTokenSource;
		IMessageBus messageBus;
		TTestCase testCase;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestCaseRunner{TTestCase}"/> class.
		/// </summary>
		/// <param name="testAssemblyUniqueID">The test assembly unique ID.</param>
		/// <param name="testCollectionUniqueID">The test collection unique ID.</param>
		/// <param name="testClassUniqueID">The test class unique ID.</param>
		/// <param name="testMethodUniqueID">The test method unique ID.</param>
		/// <param name="testCase">The test case to be run.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		protected TestCaseRunner(
			string testAssemblyUniqueID,
			string testCollectionUniqueID,
			string? testClassUniqueID,
			string? testMethodUniqueID,
			TTestCase testCase,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
		{
			this.testCase = Guard.ArgumentNotNull(nameof(testCase), testCase);
			this.messageBus = Guard.ArgumentNotNull(nameof(messageBus), messageBus);
			this.aggregator = Guard.ArgumentNotNull(nameof(aggregator), aggregator);
			this.cancellationTokenSource = Guard.ArgumentNotNull(nameof(cancellationTokenSource), cancellationTokenSource);

			TestAssemblyUniqueID = Guard.ArgumentNotNull(nameof(testAssemblyUniqueID), testAssemblyUniqueID);
			TestCollectionUniqueID = Guard.ArgumentNotNull(nameof(testCollectionUniqueID), testCollectionUniqueID);
			TestClassUniqueID = testClassUniqueID;
			TestMethodUniqueID = testMethodUniqueID;
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
		/// Gets the test assembly unique ID.
		/// </summary>
		protected string TestAssemblyUniqueID { get; }

		/// <summary>
		/// Gets or sets the test case to be run.
		/// </summary>
		protected TTestCase TestCase
		{
			get => testCase;
			set => testCase = Guard.ArgumentNotNull(nameof(TestCase), value);
		}

		/// <summary>
		/// Gets the test class unique ID.
		/// </summary>
		protected string? TestClassUniqueID { get; }

		/// <summary>
		/// Gets the test collection unique ID.
		/// </summary>
		protected string TestCollectionUniqueID { get; }

		/// <summary>
		/// Gets the test method unique ID.
		/// </summary>
		protected string? TestMethodUniqueID { get; }

		/// <summary>
		/// This method is called just after <see cref="_TestCaseStarting"/> is sent, but before any test collections are run.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual Task AfterTestCaseStartingAsync() => Task.CompletedTask;

		/// <summary>
		/// This method is called just before <see cref="_TestCaseFinished"/> is sent.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual Task BeforeTestCaseFinishedAsync() => Task.CompletedTask;

		/// <summary>
		/// Runs the tests in the test case.
		/// </summary>
		/// <returns>Returns summary information about the tests that were run.</returns>
		public async Task<RunSummary> RunAsync()
		{
			var summary = new RunSummary();

			var testCaseStarting = new _TestCaseStarting
			{
				AssemblyUniqueID = TestAssemblyUniqueID,
				SkipReason = TestCase.SkipReason,
				SourceFilePath = TestCase.SourceInformation?.FileName,
				SourceLineNumber = TestCase.SourceInformation?.LineNumber,
				TestCaseDisplayName = TestCase.DisplayName,
				TestCaseUniqueID = TestCase.UniqueID,
				TestClassUniqueID = TestClassUniqueID,
				TestCollectionUniqueID = TestCollectionUniqueID,
				TestMethodUniqueID = TestMethodUniqueID,
				Traits = TestCase.Traits
			};

			if (!MessageBus.QueueMessage(testCaseStarting))
				CancellationTokenSource.Cancel();
			else
			{
				try
				{
					await AfterTestCaseStartingAsync();
					summary = await RunTestAsync();

					Aggregator.Clear();
					await BeforeTestCaseFinishedAsync();

					if (Aggregator.HasExceptions)
					{
						var testCaseCleanupFailure = _TestCaseCleanupFailure.FromException(
							Aggregator.ToException()!,
							TestAssemblyUniqueID,
							TestCollectionUniqueID,
							TestClassUniqueID,
							TestMethodUniqueID,
							TestCase.UniqueID
						);

						if (!MessageBus.QueueMessage(testCaseCleanupFailure))
							CancellationTokenSource.Cancel();
					}
				}
				finally
				{
					var testCaseFinished = new _TestCaseFinished
					{
						AssemblyUniqueID = TestAssemblyUniqueID,
						ExecutionTime = summary.Time,
						TestCaseUniqueID = TestCase.UniqueID,
						TestClassUniqueID = TestClassUniqueID,
						TestCollectionUniqueID = TestCollectionUniqueID,
						TestMethodUniqueID = TestMethodUniqueID,
						TestsFailed = summary.Failed,
						TestsRun = summary.Total,
						TestsSkipped = summary.Skipped
					};

					if (!MessageBus.QueueMessage(testCaseFinished))
						CancellationTokenSource.Cancel();
				}
			}

			return summary;
		}

		/// <summary>
		/// Override this method to run the tests in an individual test method.
		/// </summary>
		/// <returns>Returns summary information about the tests that were run.</returns>
		protected abstract Task<RunSummary> RunTestAsync();
	}
}
