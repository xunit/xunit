using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running tests in a test method.
/// </summary>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="_ITestCase"/>.</typeparam>
public abstract class TestMethodRunner<TContext, TTestCase>
	where TContext : TestMethodRunnerContext<TTestCase>
	where TTestCase : _ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestMethodRunner{TContext, TTestCase}"/> class.
	/// </summary>
	protected TestMethodRunner()
	{ }

	/// <summary>
	/// This method is called just after <see cref="_TestMethodStarting"/> is sent, but before any test cases are run.
	/// This method should NEVER throw; any exceptions should be placed into the aggregator in <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test method</param>
	protected virtual ValueTask AfterTestMethodStarting(TContext ctxt) =>
		default;

	/// <summary>
	/// This method is called after all test cases are run, but just before <see cref="_TestMethodFinished"/> is sent.
	/// This method should NEVER throw; any exceptions should be placed into the aggregator in <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test method</param>
	protected virtual ValueTask BeforeTestMethodFinished(TContext ctxt) =>
		default;

	/// <summary>
	/// Runs the tests in the test method.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected async ValueTask<RunSummary> RunAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		var methodSummary = new RunSummary();
		var testCollection = ctxt.TestCases.First().TestCollection;
		var testAssemblyUniqueID = testCollection.TestAssembly.UniqueID;
		var testCollectionUniqueID = testCollection.UniqueID;
		var testClassUniqueID = ctxt.TestClass.UniqueID;
		var testMethodUniqueID = ctxt.TestMethod.UniqueID;

		var methodStarting = new _TestMethodStarting
		{
			AssemblyUniqueID = testAssemblyUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestMethod = ctxt.TestMethod.Method.Name,
			TestMethodUniqueID = testMethodUniqueID
		};

		if (!ctxt.MessageBus.QueueMessage(methodStarting))
		{
			ctxt.CancellationTokenSource.Cancel();
			return methodSummary;
		}

		try
		{
			await AfterTestMethodStarting(ctxt);

			SetTestContext(ctxt, TestEngineStatus.Running);

			methodSummary = await RunTestCasesAsync(ctxt);

			SetTestContext(ctxt, TestEngineStatus.CleaningUp);

			ctxt.Aggregator.Clear();
			await BeforeTestMethodFinished(ctxt);

			if (ctxt.Aggregator.HasExceptions)
			{
				var methodCleanupFailure = _TestMethodCleanupFailure.FromException(ctxt.Aggregator.ToException()!, testAssemblyUniqueID, testCollectionUniqueID, testClassUniqueID, testMethodUniqueID);
				if (!ctxt.MessageBus.QueueMessage(methodCleanupFailure))
					ctxt.CancellationTokenSource.Cancel();
			}

			return methodSummary;
		}
		finally
		{
			var testMethodFinished = new _TestMethodFinished
			{
				AssemblyUniqueID = testAssemblyUniqueID,
				ExecutionTime = methodSummary.Time,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestsFailed = methodSummary.Failed,
				TestsNotRun = methodSummary.NotRun,
				TestsTotal = methodSummary.Total,
				TestsSkipped = methodSummary.Skipped
			};

			if (!ctxt.MessageBus.QueueMessage(testMethodFinished))
				ctxt.CancellationTokenSource.Cancel();
		}
	}

	/// <summary>
	/// Runs the list of test cases. By default, it runs the cases in order, synchronously.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected virtual async ValueTask<RunSummary> RunTestCasesAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var summary = new RunSummary();

		foreach (var testCase in ctxt.TestCases)
		{
			summary.Aggregate(await RunTestCaseAsync(ctxt, testCase));
			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				break;
		}

		return summary;
	}

	/// <summary>
	/// Override this method to run an individual test case.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <param name="testCase">The test case to be run.</param>
	/// <returns>Returns summary information about the test case run.</returns>
	protected abstract ValueTask<RunSummary> RunTestCaseAsync(
		TContext ctxt,
		TTestCase testCase);

	/// <summary>
	/// Sets the current <see cref="TestContext"/> for the current test method and the given test method status.
	/// Does nothing when <see cref="TestMethod"/> is <c>null</c>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <param name="testMethodStatus">The current test method status.</param>
	protected virtual void SetTestContext(
		TContext ctxt,
		TestEngineStatus testMethodStatus)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTestMethod(ctxt.TestMethod, testMethodStatus, ctxt.CancellationTokenSource.Token);
	}
}
