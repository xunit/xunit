using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running a test. This includes support
/// for skipping tests.
/// </summary>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTest">The test type used by the test framework. Must derive from
/// <see cref="ITest"/>.</typeparam>
public abstract class TestRunner<TContext, TTest>
	where TContext : TestRunnerContext<TTest>
	where TTest : class, ITest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestRunner{TContext, TTest}"/> class.
	/// </summary>
	protected TestRunner()
	{ }

	async ValueTask<(object?, SynchronizationContext?, ExecutionContext?)> CreateTestClass(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		if (!ctxt.Aggregator.Run(() => IsTestClassCreatable(ctxt), false))
			return (null, SynchronizationContext.Current, ExecutionContext.Capture());

		if (ctxt.CancellationTokenSource.IsCancellationRequested || ctxt.Aggregator.HasExceptions)
			return (null, SynchronizationContext.Current, ExecutionContext.Capture());

		if (!await ctxt.Aggregator.RunAsync(() => OnTestClassConstructionStarting(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		if (ctxt.CancellationTokenSource.IsCancellationRequested || ctxt.Aggregator.HasExceptions)
			return (null, SynchronizationContext.Current, ExecutionContext.Capture());

		var result = await ctxt.Aggregator.RunAsync(() => CreateTestClassInstance(ctxt), (null, null, null));

		if (!await ctxt.Aggregator.RunAsync(() => OnTestClassConstructionFinished(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		return result;
	}

	/// <summary>
	/// Override to creates and initialize the instance of the test class.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <returns>Returns the test class instance, and the sync context that is current after the creation</returns>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure. Since the method is potentially async, we depend on it to capture and
	/// return the sync context so that it may be propagated appropriately.
	/// </remarks>
	protected abstract ValueTask<(object? Instance, SynchronizationContext? SyncContext, ExecutionContext? ExecutionContext)> CreateTestClassInstance(TContext ctxt);

	async ValueTask DisposeTestClass(
		TContext ctxt,
		object? testClassInstance)
	{
		Guard.ArgumentNotNull(ctxt);

		if (testClassInstance is null || !ctxt.Aggregator.Run(() => IsTestClassDisposable(ctxt, testClassInstance), false))
			return;

		if (!await ctxt.Aggregator.RunAsync(() => OnTestClassDisposeStarting(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		await ctxt.Aggregator.RunAsync(() => DisposeTestClassInstance(ctxt, testClassInstance));

		if (!await ctxt.Aggregator.RunAsync(() => OnTestClassDisposeFinished(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();
	}

	/// <summary>
	/// Override to dispose of the test class instance.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// contribute to test cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="testClassInstance">The test class instance</param>
	protected abstract ValueTask DisposeTestClassInstance(
		TContext ctxt,
		object testClassInstance);

	/// <summary>
	/// Gets any output collected from the test after execution is complete. If the test framework
	/// did not collect any output, or does not support collecting output, then it should
	/// return <see cref="string.Empty"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// contribute to test cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	protected abstract ValueTask<string> GetTestOutput(TContext ctxt);

	/// <summary>
	/// Override this method to invoke the test.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="testClassInstance">The instance of the test class (may be <c>null</c> when
	/// running a static test method)</param>
	/// <returns>Returns the execution time (in seconds) spent running the test method.</returns>
	protected abstract ValueTask<TimeSpan> InvokeTestAsync(
		TContext ctxt,
		object? testClassInstance);

	/// <summary>
	/// Override to determine whether a test class should be created.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure (and test class creation will not take place).
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	protected abstract bool IsTestClassCreatable(TContext ctxt);

	/// <summary>
	/// Override to determine whether a test class instance should be disposed.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="testClassInstance">The test class instance</param>
	protected abstract bool IsTestClassDisposable(
		TContext ctxt,
		object testClassInstance);

	/// <summary>
	/// This method will be called when a test class instance has finished being constructed. This
	/// will typically send a message like <see cref="ITestClassConstructionFinished"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The invoker context</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestClassConstructionFinished(TContext ctxt);

	/// <summary>
	/// This method will be called when a test class instance is about to be constructed. This
	/// will typically send a message like <see cref="ITestClassConstructionStarting"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure (and test class creation will not take place).
	/// </remarks>
	/// <param name="ctxt">The invoker context</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestClassConstructionStarting(TContext ctxt);

	/// <summary>
	/// This method will be called when a test class instance has finished being disposed. This
	/// will typically send a message like <see cref="ITestClassDisposeFinished"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The invoker context</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestClassDisposeFinished(TContext ctxt);

	/// <summary>
	/// This method will be called when a test class instance is about to be disposed. This
	/// will typically send a message like <see cref="ITestClassDisposeStarting"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The invoker context</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestClassDisposeStarting(TContext ctxt);

	/// <summary>
	/// This method is called when an exception was thrown while cleaning up, after the test has run.
	/// This will typically send a "test cleanup failure" message (like <see cref="ITestCleanupFailure"/>).
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown are
	/// converted into fatal exception messages (via <see cref="IErrorMessage"/>) and sent to the message
	/// bus in <paramref name="ctxt"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="exception">The exception that caused the cleanup failure (may be an instance
	/// of <see cref="AggregateException"/> if more than one exception occurred).</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestCleanupFailure(TContext ctxt, Exception exception);

	/// <summary>
	/// This method is called when a test has failed. This will typically send a test failed message
	/// (i.e., <see cref="ITestFailed"/>).
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// contribute to test cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="exception">The exception that caused the test failure</param>
	/// <param name="executionTime">The time spent running the test</param>
	/// <param name="output">The output from the test</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<(bool Continue, TestResultState ResultState)> OnTestFailed(
		TContext ctxt,
		Exception exception,
		decimal executionTime,
		string output);

	/// <summary>
	/// This method is called just after the test has finished running. This will typically send a test finished
	/// message (i.e., <see cref="ITestFinished"/>) as well as enabling any extensibility related to test finish.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// contribute to test cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="executionTime">The time spent running the test</param>
	/// <param name="output">The output from the test</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestFinished(
		TContext ctxt,
		decimal executionTime,
		string output);

	/// <summary>
	/// This method is called when a test was not run. This will typically send a "test not run" message
	/// (like <see cref="ITestNotRun"/>).
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// contribute to test cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="output">The output from the test</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<(bool Continue, TestResultState ResultState)> OnTestNotRun(
		TContext ctxt,
		string output);

	/// <summary>
	/// This method is called when a test has passed. This will typically send a "test passed" message
	/// (like <see cref="ITestPassed"/>).
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// contribute to test cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="executionTime">The time spent running the test</param>
	/// <param name="output">The output from the test</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<(bool Continue, TestResultState ResultState)> OnTestPassed(
		TContext ctxt,
		decimal executionTime,
		string output);

	/// <summary>
	/// This method is called when a test is skipped. This will typically send a "test not run" message
	/// (like <see cref="ITestNotRun"/>).
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// contribute to test cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="skipReason">The reason given for skipping the test</param>
	/// <param name="executionTime">The time spent running the test</param>
	/// <param name="output">The output from the test</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<(bool Continue, TestResultState ResultState)> OnTestSkipped(
		TContext ctxt,
		string skipReason,
		decimal executionTime,
		string output);

	/// <summary>
	/// This method is called just before the test is run. This will typically send a "test starting" message
	/// (like <see cref="ITestStarting"/>) as well as enabling any extensibility related to test start.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Initializing"/> and any exceptions thrown will
	/// contribute to test failure (and will prevent the test from running).  Even if this method records
	/// exceptions, <see cref="OnTestFinished"/> will be called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestStarting(TContext ctxt);

	/// <summary>
	/// Override this method to call code just after the test invocation has completed, but before
	/// the test class instance has been disposed.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	protected virtual ValueTask PostInvoke(TContext ctxt) =>
		default;

	/// <summary>
	/// Override this method to call code just after the test class instance has been created, but
	/// before the test has been invoked.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	protected virtual ValueTask PreInvoke(TContext ctxt) =>
		default;

	/// <summary>
	/// Runs the test.
	/// </summary>
	/// <remarks>
	/// This function is the primary orchestrator of test execution.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	protected async ValueTask<RunSummary> RunAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		var summary = new RunSummary();
		var output = string.Empty;
		var elapsedTime = TimeSpan.Zero;

		if (!await ctxt.Aggregator.RunAsync(() => OnTestStarting(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		var @continue = true;
		TestResultState? resultState = null;

		SetTestContext(ctxt, TestEngineStatus.Running);

		if (!ctxt.CancellationTokenSource.IsCancellationRequested)
		{
			object? testClassInstance = null;
			var shouldRun = true;

			summary.Total = 1;

			if (!ctxt.Aggregator.HasExceptions)
			{
				shouldRun = ctxt.Aggregator.Run(() => ShouldTestRun(ctxt), true);

				if (shouldRun && ctxt.GetSkipReason() is null && !ctxt.Aggregator.HasExceptions)
				{
					SynchronizationContext? syncContext = null;
					ExecutionContext? executionContext = null;

					elapsedTime += await ExecutionTimer.MeasureAsync(() => ctxt.Aggregator.RunAsync(async () => { (testClassInstance, syncContext, executionContext) = await CreateTestClass(ctxt); }));

					TaskCompletionSource<object?> finished = new();

					if (executionContext is not null)
						ExecutionContext.Run(executionContext, runTest, null);
					else
						runTest(null);

					await finished.Task;

					async void runTest(object? _)
					{
						SynchronizationContext.SetSynchronizationContext(syncContext);
						SetTestContext(ctxt, TestEngineStatus.Running, testClassInstance: testClassInstance);

						try
						{
							if (!ctxt.Aggregator.HasExceptions)
							{
								elapsedTime += await ExecutionTimer.MeasureAsync(() => ctxt.Aggregator.RunAsync(() => PreInvoke(ctxt)));

								if (!ctxt.Aggregator.HasExceptions)
								{
									elapsedTime += await ctxt.Aggregator.RunAsync(() => InvokeTestAsync(ctxt, testClassInstance), TimeSpan.Zero);

									// Set an early version of TestResultState so anything done in PostInvoke can understand whether
									// it looks like the test is passing, failing, or dynamically skipped
									var currentException = ctxt.Aggregator.ToException();
									var currentSkipReason = ctxt.GetSkipReason(currentException);
									var currentExecutionTime = (decimal)elapsedTime.TotalMilliseconds;
									var testResultState =
										currentSkipReason is not null
											? TestResultState.ForSkipped(currentExecutionTime)
											: TestResultState.FromException(currentExecutionTime, currentException);

									SetTestContext(ctxt, TestEngineStatus.Running, testResultState, testClassInstance: testClassInstance);

									elapsedTime += await ExecutionTimer.MeasureAsync(() => ctxt.Aggregator.RunAsync(() => PostInvoke(ctxt)));
								}

								elapsedTime += await ExecutionTimer.MeasureAsync(() => ctxt.Aggregator.RunAsync(() => DisposeTestClass(ctxt, testClassInstance)));

								SetTestContext(ctxt, TestEngineStatus.Running, TestContext.Current.TestState, testClassInstance: null);
							}

							finished.TrySetResult(null);
						}
						catch (Exception ex)
						{
							finished.TrySetException(ex);
						}
					}
				}
			}

			output = await ctxt.Aggregator.RunAsync(() => GetTestOutput(ctxt), string.Empty);
			summary.Time = (decimal)elapsedTime.TotalSeconds;

			var exception = ctxt.Aggregator.ToException();
			var skipReason = ctxt.GetSkipReason(exception);

			ctxt.Aggregator.Clear();

			if (!shouldRun)
			{
				summary.NotRun++;
				(@continue, resultState) = await ctxt.Aggregator.RunAsync(() => OnTestNotRun(ctxt, output), (true, TestResultState.ForNotRun()));
			}
			else if (skipReason is not null)
			{
				summary.Skipped++;
				(@continue, resultState) = await ctxt.Aggregator.RunAsync(() => OnTestSkipped(ctxt, skipReason, 0m, output), (true, TestResultState.ForNotRun()));
			}
			else if (exception is not null)
			{
				summary.Failed++;
				(@continue, resultState) = await ctxt.Aggregator.RunAsync(() => OnTestFailed(ctxt, exception, summary.Time, output), (true, TestResultState.ForNotRun()));
			}
			else
				(@continue, resultState) = await ctxt.Aggregator.RunAsync(() => OnTestPassed(ctxt, summary.Time, output), (true, TestResultState.ForNotRun()));
		}

		if (!@continue)
			ctxt.CancellationTokenSource.Cancel();

		SetTestContext(ctxt, TestEngineStatus.CleaningUp, resultState ?? TestResultState.ForNotRun());

		if (!await ctxt.Aggregator.RunAsync(() => OnTestFinished(ctxt, summary.Time, output), true))
			ctxt.CancellationTokenSource.Cancel();

		if (ctxt.Aggregator.HasExceptions)
		{
			var exception = ctxt.Aggregator.ToException()!;
			ctxt.Aggregator.Clear();

			if (!await ctxt.Aggregator.RunAsync(() => OnTestCleanupFailure(ctxt, exception), true))
				ctxt.CancellationTokenSource.Cancel();

			if (ctxt.Aggregator.HasExceptions)
				ctxt.MessageBus.QueueMessage(ErrorMessage.FromException(ctxt.Aggregator.ToException()!));

			ctxt.Aggregator.Clear();
		}

		return summary;
	}

	/// <summary>
	/// Sets the test context for the given test state and engine status.
	/// </summary>
	/// <remarks>
	/// This method must never throw. Behavior is undefined if it does. Instead, exceptions that
	/// occur should be recorded in the aggregator in <paramref name="ctxt"/> and will be reflected
	/// in a way that's appropriate based on when this method is called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="testStatus">The current engine status for the test</param>
	/// <param name="testState">The current test state</param>
	/// <param name="testClassInstance">The instance of the test class</param>
	protected virtual void SetTestContext(
		TContext ctxt,
		TestEngineStatus testStatus,
		TestResultState? testState = null,
		object? testClassInstance = null)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTest(ctxt.Test, testStatus, ctxt.CancellationTokenSource.Token, testState, testClassInstance: testClassInstance);
	}

	/// <summary>
	/// Override this to determine whether a test should be run or not (meaning, if you return <c>false</c>,
	/// it will be reported with a status of <see cref="TestResult.NotRun"/>). By default, this method will
	/// return <c>true</c>. This is typically used to implement the ability to exclude specific tests
	/// unless they've been explicitly asked to be run.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	protected virtual bool ShouldTestRun(TContext ctxt) =>
		true;
}
