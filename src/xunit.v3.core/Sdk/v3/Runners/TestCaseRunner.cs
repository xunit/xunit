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
		/// <param name="testCase">The test case to be run.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		protected TestCaseRunner(
			TTestCase testCase,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
		{
			this.testCase = Guard.ArgumentNotNull(nameof(testCase), testCase);
			this.messageBus = Guard.ArgumentNotNull(nameof(messageBus), messageBus);
			this.aggregator = Guard.ArgumentNotNull(nameof(aggregator), aggregator);
			this.cancellationTokenSource = Guard.ArgumentNotNull(nameof(cancellationTokenSource), cancellationTokenSource);
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
		/// Gets or sets the test case to be run.
		/// </summary>
		protected TTestCase TestCase
		{
			get => testCase;
			set => testCase = Guard.ArgumentNotNull(nameof(TestCase), value);
		}

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
			SetTestContext(TestEngineStatus.Initializing);

			var summary = new RunSummary();

			var testCaseStarting = new _TestCaseStarting
			{
				AssemblyUniqueID = TestCase.TestCollection.TestAssembly.UniqueID,
				SkipReason = TestCase.SkipReason,
				SourceFilePath = TestCase.SourceFilePath,
				SourceLineNumber = TestCase.SourceLineNumber,
				TestCaseDisplayName = TestCase.TestCaseDisplayName,
				TestCaseUniqueID = TestCase.UniqueID,
				TestClassUniqueID = TestCase.TestMethod?.TestClass.UniqueID,
				TestCollectionUniqueID = TestCase.TestCollection.UniqueID,
				TestMethodUniqueID = TestCase.TestMethod?.UniqueID,
				Traits = TestCase.Traits
			};

			if (!MessageBus.QueueMessage(testCaseStarting))
				CancellationTokenSource.Cancel();
			else
			{
				try
				{
					await AfterTestCaseStartingAsync();

					SetTestContext(TestEngineStatus.Running);

					summary = await RunTestAsync();

					SetTestContext(TestEngineStatus.CleaningUp);

					Aggregator.Clear();
					await BeforeTestCaseFinishedAsync();

					if (Aggregator.HasExceptions)
					{
						var testCaseCleanupFailure = _TestCaseCleanupFailure.FromException(
							Aggregator.ToException()!,
							TestCase.TestCollection.TestAssembly.UniqueID,
							TestCase.TestCollection.UniqueID,
							TestCase.TestMethod?.TestClass.UniqueID,
							TestCase.TestMethod?.UniqueID,
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
						AssemblyUniqueID = TestCase.TestCollection.TestAssembly.UniqueID,
						ExecutionTime = summary.Time,
						TestCaseUniqueID = TestCase.UniqueID,
						TestClassUniqueID = TestCase.TestMethod?.TestClass.UniqueID,
						TestCollectionUniqueID = TestCase.TestCollection.UniqueID,
						TestMethodUniqueID = TestCase.TestMethod?.UniqueID,
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

		/// <summary>
		/// Sets the current <see cref="TestContext"/> for the current test case and the given test case status.
		/// </summary>
		/// <param name="testCaseStatus">The current test case status.</param>
		protected virtual void SetTestContext(TestEngineStatus testCaseStatus) =>
			TestContext.SetForTestCase(TestCase, testCaseStatus);
	}
}
