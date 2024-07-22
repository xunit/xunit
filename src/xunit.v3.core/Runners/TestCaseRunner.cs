using System;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running test cases.
/// </summary>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
public abstract class TestCaseRunner<TContext, TTestCase>
	where TContext : TestCaseRunnerContext<TTestCase>
	where TTestCase : class, ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestCaseRunner{TContext, TTestCase}"/> class.
	/// </summary>
	protected TestCaseRunner()
	{ }

	/// <summary>
	/// This method is called when an exception was thrown while cleaning up, after the test
	/// case has run. This will typically send a "test case cleanup failure" message (like
	/// <see cref="ITestCaseCleanupFailure"/>).
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown are
	/// converted into fatal exception messages (via <see cref="IErrorMessage"/>) and sent to the message
	/// bus in <paramref name="ctxt"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test case</param>
	/// <param name="exception">The exception that caused the cleanup failure (may be an instance
	/// of <see cref="AggregateException"/> if more than one exception occurred).</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestCaseCleanupFailure(
		TContext ctxt,
		Exception exception);

	/// <summary>
	/// This method will be called when the test case has finished running. This will typically send
	/// a "test case finished" message (like <see cref="ITestCaseFinished"/>)  as well as enabling any
	/// extensibility related to test case finish.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// contribute to test case cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test case</param>
	/// <param name="summary">The execution summary for the test case.</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestCaseFinished(
		TContext ctxt,
		RunSummary summary);

	/// <summary>
	/// This method will be called before the test case has started running. This will typically send
	/// a "test case starting" message (like <see cref="ITestCaseStarting"/>) as well as enabling any
	/// extensibility related to test case start.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Initializing"/> and any exceptions thrown will
	/// contribute to test case failure (and will prevent the test case from running). Even if
	/// this method records exceptions, <see cref="OnTestCaseFinished"/> will be called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test case</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestCaseStarting(TContext ctxt);

	/// <summary>
	/// Runs the tests in the test case.
	/// </summary>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected async ValueTask<RunSummary> RunAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		var summary = default(RunSummary);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestCaseStarting(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		SetTestContext(ctxt, TestEngineStatus.Running);

		var startupException = ctxt.Aggregator.ToException();
		ctxt.Aggregator.Clear();

		if (!ctxt.CancellationTokenSource.IsCancellationRequested)
			summary = await ctxt.Aggregator.RunAsync(() => RunTestsAsync(ctxt, startupException), default);

		SetTestContext(ctxt, TestEngineStatus.CleaningUp);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestCaseFinished(ctxt, summary), true))
			ctxt.CancellationTokenSource.Cancel();

		if (ctxt.Aggregator.HasExceptions)
		{
			var exception = ctxt.Aggregator.ToException()!;
			ctxt.Aggregator.Clear();

			if (!await ctxt.Aggregator.RunAsync(() => OnTestCaseCleanupFailure(ctxt, exception), true))
				ctxt.CancellationTokenSource.Cancel();

			if (ctxt.Aggregator.HasExceptions)
				ctxt.MessageBus.QueueMessage(ErrorMessage.FromException(ctxt.Aggregator.ToException()!));

			ctxt.Aggregator.Clear();
		}

		return summary;
	}

	/// <summary>
	/// Override this method to run the tests in an individual test case.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and should never throw (any
	/// exceptions thrown should be caught and recorded as test case failures). If any exception happens
	/// to be thrown here, it will be recorded as a test case cleanup failure, which is probably
	/// not what the end user will be expecting.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test case</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected abstract ValueTask<RunSummary> RunTestsAsync(
		TContext ctxt,
		Exception? exception);

	/// <summary>
	/// Sets the current <see cref="TestContext"/> for the current test case and the given test case status.
	/// </summary>
	/// <remarks>
	/// This method must never throw. Behavior is undefined if it does. Instead, exceptions that
	/// occur should be recorded in the aggregator in <paramref name="ctxt"/> and will be reflected
	/// in a way that's appropriate based on when this method is called.
	/// </remarks>
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
