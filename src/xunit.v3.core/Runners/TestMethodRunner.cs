using System;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running tests in a test method.
/// </summary>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTestMethod">The type of the test method used by the test framework.
/// Must derive from <see cref="ITestMethod"/>.</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
public abstract class TestMethodRunner<TContext, TTestMethod, TTestCase>
	where TContext : TestMethodRunnerContext<TTestMethod, TTestCase>
	where TTestMethod : class, ITestMethod
	where TTestCase : class, ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestMethodRunner{TContext, TTestCase, TTestMethod}"/> class.
	/// </summary>
	protected TestMethodRunner()
	{ }

	/// <summary>
	/// This method is called when an exception was thrown while cleaning up, after the test method
	/// has run. This will typically send a "test method cleanup failure" message (like
	/// <see cref="ITestMethodCleanupFailure"/>).
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown are
	/// converted into fatal exception messages (via <see cref="IErrorMessage"/>) and sent to the message
	/// bus in <paramref name="ctxt"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <param name="exception">The exception that caused the cleanup failure (may be an instance
	/// of <see cref="AggregateException"/> if more than one exception occurred).</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestMethodCleanupFailure(
		TContext ctxt,
		Exception exception);

	/// <summary>
	/// This method will be called when the test method has finished running. This will typically send
	/// a "test method finished" message (like <see cref="ITestMethodFinished"/>) as well as enabling any
	/// extensibility related to test method finish.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// contribute to test method cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <param name="summary">The execution summary for the test method</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestMethodFinished(
		TContext ctxt,
		RunSummary summary);

	/// <summary>
	/// This method will be called before the test method has started running. This will typically send
	/// a "test method starting" message (like <see cref="ITestMethodStarting"/>) as well as enabling any
	/// extensibility related to test method start.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Initializing"/> and any exceptions thrown will
	/// contribute to test method failure (and will prevent the test method from running). Even if
	/// this method records exceptions, <see cref="OnTestMethodFinished"/> will be called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestMethodStarting(TContext ctxt);

	/// <summary>
	/// Runs the tests in the test method.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected async ValueTask<RunSummary> RunAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		var summary = default(RunSummary);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestMethodStarting(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		SetTestContext(ctxt, TestEngineStatus.Running);

		var startingException = ctxt.Aggregator.ToException();
		ctxt.Aggregator.Clear();

		if (!ctxt.CancellationTokenSource.IsCancellationRequested)
			summary = await ctxt.Aggregator.RunAsync(() => RunTestCasesAsync(ctxt, startingException), default);

		SetTestContext(ctxt, TestEngineStatus.CleaningUp);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestMethodFinished(ctxt, summary), true))
			ctxt.CancellationTokenSource.Cancel();

		if (ctxt.Aggregator.HasExceptions)
		{
			var exception = ctxt.Aggregator.ToException()!;
			ctxt.Aggregator.Clear();

			if (!await ctxt.Aggregator.RunAsync(() => OnTestMethodCleanupFailure(ctxt, exception), true))
				ctxt.CancellationTokenSource.Cancel();

			if (ctxt.Aggregator.HasExceptions)
				ctxt.MessageBus.QueueMessage(ErrorMessage.FromException(ctxt.Aggregator.ToException()!));

			ctxt.Aggregator.Clear();
		}

		return summary;
	}

	/// <summary>
	/// Runs the list of test cases. By default, it runs the cases in order, synchronously.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected virtual async ValueTask<RunSummary> RunTestCasesAsync(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var summary = new RunSummary();

		foreach (var testCase in ctxt.TestCases)
		{
			summary.Aggregate(await RunTestCaseAsync(ctxt, testCase, exception));
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
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the test case run.</returns>
	protected abstract ValueTask<RunSummary> RunTestCaseAsync(
		TContext ctxt,
		TTestCase testCase,
		Exception? exception);

	/// <summary>
	/// Sets the current <see cref="TestContext"/> for the current test method and the given test method status.
	/// </summary>
	/// <remarks>
	/// This method must never throw. Behavior is undefined if it does. Instead, exceptions that
	/// occur should be recorded in the aggregator in <paramref name="ctxt"/> and will be reflected
	/// in a way that's appropriate based on when this method is called.
	/// </remarks>
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
