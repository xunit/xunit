using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running test cases.
/// </summary>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="_ITestCase"/>.</typeparam>
public abstract class TestCaseRunner<TContext, TTestCase>
	where TContext : TestCaseRunnerContext<TTestCase>
	where TTestCase : class, _ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestCaseRunner{TContext, TTestCase}"/> class.
	/// </summary>
	protected TestCaseRunner()
	{ }

	/// <summary>
	/// This method is called just after <see cref="_TestCaseStarting"/> is sent, but before the test case is run.
	/// This method should NEVER throw; any exceptions should be placed into the aggregator in <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test case</param>
	protected virtual ValueTask AfterTestCaseStartingAsync(TContext ctxt) =>
		default;

	/// <summary>
	/// This method is called after the test case is run, but just before <see cref="_TestCaseFinished"/> is sent.
	/// This method should NEVER throw; any exceptions should be placed into the aggregator in <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test case</param>
	protected virtual ValueTask BeforeTestCaseFinishedAsync(TContext ctxt) =>
		default;

	/// <summary>
	/// Runs the tests in the test case.
	/// </summary>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected async ValueTask<RunSummary> RunAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		var summary = new RunSummary();

		var assemblyUniqueID = ctxt.TestCase.TestCollection.TestAssembly.UniqueID;
		var testCollectionUniqueID = ctxt.TestCase.TestCollection.UniqueID;
		var testClassUniqueID = ctxt.TestCase.TestClass?.UniqueID;
		var testMethodUniqueID = ctxt.TestCase.TestMethod?.UniqueID;
		var testCaseUniqueID = ctxt.TestCase.UniqueID;

		var testCaseStarting = new _TestCaseStarting
		{
			AssemblyUniqueID = assemblyUniqueID,
			SkipReason = ctxt.TestCase.SkipReason,
			SourceFilePath = ctxt.TestCase.SourceFilePath,
			SourceLineNumber = ctxt.TestCase.SourceLineNumber,
			TestCaseDisplayName = ctxt.TestCase.TestCaseDisplayName,
			TestCaseUniqueID = testCaseUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			Traits = ctxt.TestCase.Traits
		};

		if (!ctxt.MessageBus.QueueMessage(testCaseStarting))
			ctxt.CancellationTokenSource.Cancel();
		else
		{
			try
			{
				await AfterTestCaseStartingAsync(ctxt);

				SetTestContext(ctxt, TestEngineStatus.Running);

				summary = await RunTestsAsync(ctxt);

				SetTestContext(ctxt, TestEngineStatus.CleaningUp);

				ctxt.Aggregator.Clear();
				await BeforeTestCaseFinishedAsync(ctxt);

				if (ctxt.Aggregator.HasExceptions)
				{
					var testCaseCleanupFailure = _TestCaseCleanupFailure.FromException(
						ctxt.Aggregator.ToException()!,
						assemblyUniqueID,
						testCollectionUniqueID,
						testClassUniqueID,
						testMethodUniqueID,
						testCaseUniqueID
					);

					if (!ctxt.MessageBus.QueueMessage(testCaseCleanupFailure))
						ctxt.CancellationTokenSource.Cancel();
				}
			}
			finally
			{
				var testCaseFinished = new _TestCaseFinished
				{
					AssemblyUniqueID = assemblyUniqueID,
					ExecutionTime = summary.Time,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestsFailed = summary.Failed,
					TestsNotRun = summary.NotRun,
					TestsSkipped = summary.Skipped,
					TestsTotal = summary.Total,
				};

				if (!ctxt.MessageBus.QueueMessage(testCaseFinished))
					ctxt.CancellationTokenSource.Cancel();
			}
		}

		return summary;
	}

	/// <summary>
	/// Override this method to run the tests in an individual test case.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test case</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected abstract ValueTask<RunSummary> RunTestsAsync(TContext ctxt);

	/// <summary>
	/// Sets the current <see cref="TestContext"/> for the current test case and the given test case status.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test case</param>
	/// <param name="testCaseStatus">The current test case status.</param>
	protected virtual void SetTestContext(
		TContext ctxt,
		TestEngineStatus testCaseStatus)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTestCase(ctxt.TestCase, testCaseStatus, ctxt.CancellationTokenSource.Token);
	}
}
