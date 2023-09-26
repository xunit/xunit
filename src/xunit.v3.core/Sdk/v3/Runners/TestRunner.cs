using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running a test. This includes support
/// for skipping tests.
/// </summary>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
public abstract class TestRunner<TContext>
	where TContext : TestRunnerContext
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestRunner{TContext}"/> class.
	/// </summary>
	protected TestRunner()
	{ }

	/// <summary>
	/// This method is called just after <see cref="_TestStarting"/> is sent, but before the test class is created.
	/// This method should NEVER throw; any exceptions should be placed into the aggregator of <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	protected virtual void AfterTestStarting(TContext ctxt)
	{ }

	/// <summary>
	/// This method is called just before <see cref="_TestFinished"/> is sent.
	/// This method should NEVER throw; any exceptions should be placed into the aggregator of <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	protected virtual void BeforeTestFinished(TContext ctxt)
	{ }

	/// <summary>
	/// Override this method to invoke the test.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <returns>Returns a tuple which includes the execution time (in seconds) spent running the
	/// test method, and any output that was returned by the test.</returns>
	protected abstract ValueTask<(decimal ExecutionTime, string Output)?> InvokeTestAsync(TContext ctxt);

	/// <summary>
	/// Runs the test.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	protected async ValueTask<RunSummary> RunAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		var runSummary = new RunSummary { Total = 1 };
		var output = string.Empty;

		var testAssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID;
		var testCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID;
		var testClassUniqueID = ctxt.Test.TestCase.TestClass?.UniqueID;
		var testMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID;
		var testCaseUniqueID = ctxt.Test.TestCase.UniqueID;
		var testUniqueID = ctxt.Test.UniqueID;

		var testStarting = new _TestStarting
		{
			AssemblyUniqueID = testAssemblyUniqueID,
			Explicit = ctxt.Test.Explicit,
			TestCaseUniqueID = testCaseUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestDisplayName = ctxt.Test.TestDisplayName,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
			Timeout = ctxt.Test.Timeout,
			Traits = ctxt.Test.Traits,
		};

		if (!ctxt.MessageBus.QueueMessage(testStarting))
			ctxt.CancellationTokenSource.Cancel();
		else
		{
			AfterTestStarting(ctxt);

			_TestResultMessage testResult;

			var shouldRun = ctxt.ExplicitOption switch
			{
				ExplicitOption.Only => ctxt.Test.Explicit,
				ExplicitOption.Off => !ctxt.Test.Explicit,
				_ => true,
			};

			if (!shouldRun)
			{
				runSummary.NotRun++;

				testResult = new _TestNotRun
				{
					AssemblyUniqueID = testAssemblyUniqueID,
					ExecutionTime = 0m,
					Output = "",
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestUniqueID = testUniqueID,
					Warnings = TestContext.Current?.Warnings?.ToArray(),
				};
			}
			else if (!string.IsNullOrEmpty(ctxt.SkipReason))
			{
				runSummary.Skipped++;

				testResult = new _TestSkipped
				{
					AssemblyUniqueID = testAssemblyUniqueID,
					ExecutionTime = 0m,
					Output = "",
					Reason = ctxt.SkipReason,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestUniqueID = testUniqueID,
					Warnings = TestContext.Current?.Warnings?.ToArray(),
				};
			}
			else
			{
				if (!ctxt.Aggregator.HasExceptions)
				{
					var tuple = await ctxt.Aggregator.RunAsync(() => InvokeTestAsync(ctxt), (0m, string.Empty));
					if (tuple.HasValue)
					{
						runSummary.Time = tuple.Value.ExecutionTime;
						output = tuple.Value.Output;
					}
				}

				var exception = ctxt.Aggregator.ToException();

				if (exception is null)
				{
					testResult = new _TestPassed
					{
						AssemblyUniqueID = testAssemblyUniqueID,
						ExecutionTime = runSummary.Time,
						Output = output,
						TestCaseUniqueID = testCaseUniqueID,
						TestClassUniqueID = testClassUniqueID,
						TestCollectionUniqueID = testCollectionUniqueID,
						TestMethodUniqueID = testMethodUniqueID,
						TestUniqueID = testUniqueID,
						Warnings = TestContext.Current?.Warnings?.ToArray(),
					};
				}
				// We don't want a strongly typed contract here; any exception can be a dynamically
				// skipped exception so long as its message starts with the special token.
				else if (exception.Message.StartsWith(DynamicSkipToken.Value, StringComparison.Ordinal))
				{
					testResult = new _TestSkipped
					{
						AssemblyUniqueID = testAssemblyUniqueID,
						ExecutionTime = runSummary.Time,
						Output = output,
						Reason = exception.Message.Substring(DynamicSkipToken.Value.Length),
						TestCaseUniqueID = testCaseUniqueID,
						TestClassUniqueID = testClassUniqueID,
						TestCollectionUniqueID = testCollectionUniqueID,
						TestMethodUniqueID = testMethodUniqueID,
						TestUniqueID = testUniqueID,
						Warnings = TestContext.Current?.Warnings?.ToArray(),
					};
					runSummary.Skipped++;
				}
				else
				{
					testResult = _TestFailed.FromException(
						exception,
						testAssemblyUniqueID,
						testCollectionUniqueID,
						testClassUniqueID,
						testMethodUniqueID,
						testCaseUniqueID,
						testUniqueID,
						runSummary.Time,
						output,
						TestContext.Current?.Warnings?.ToArray()
					);
					runSummary.Failed++;
				}
			}

			SetTestContext(ctxt, TestEngineStatus.CleaningUp, TestResultState.FromTestResult(testResult));

			if (!ctxt.CancellationTokenSource.IsCancellationRequested)
				if (!ctxt.MessageBus.QueueMessage(testResult))
					ctxt.CancellationTokenSource.Cancel();

			ctxt.Aggregator.Clear();
			BeforeTestFinished(ctxt);

			if (ctxt.Aggregator.HasExceptions)
			{
				var testCleanupFailure = _TestCleanupFailure.FromException(
					ctxt.Aggregator.ToException()!,
					testAssemblyUniqueID,
					testCollectionUniqueID,
					testClassUniqueID,
					testMethodUniqueID,
					testCaseUniqueID,
					testUniqueID
				);

				if (!ctxt.MessageBus.QueueMessage(testCleanupFailure))
					ctxt.CancellationTokenSource.Cancel();
			}

			var testFinished = new _TestFinished
			{
				AssemblyUniqueID = testAssemblyUniqueID,
				ExecutionTime = runSummary.Time,
				Output = output,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};

			if (!ctxt.MessageBus.QueueMessage(testFinished))
				ctxt.CancellationTokenSource.Cancel();
		}

		return runSummary;
	}

	/// <summary>
	/// Sets the test context for the given test state and engine status.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="testStatus">The current engine status for the test</param>
	/// <param name="testState">The current test state</param>
	protected virtual void SetTestContext(
		TContext ctxt,
		TestEngineStatus testStatus,
		TestResultState? testState = null)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTest(ctxt.Test, testStatus, ctxt.CancellationTokenSource.Token, testState);
	}
}
