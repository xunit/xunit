using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base test assembly runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is shared between reflection-based and code generation-based tests.
/// </remarks>
public class CoreTestCaseRunner<TContext, TTestCase, TTest> : TestCaseRunner<TContext, TTestCase, TTest>
	where TContext : CoreTestCaseRunnerContext<TTestCase, TTest>
	where TTestCase : class, ICoreTestCase
	where TTest : class, ICoreTest
{
	/// <summary>
	/// Calls <see cref="RunTestCaseInner"/>, wrapped in calls to <see cref="ICoreTestCase.PreInvoke"/>
	/// and <see cref="ICoreTestCase.PostInvoke"/> (when there isn't a pre-existing exception).
	/// </summary>
	/// <inheritdoc/>
	protected override sealed async ValueTask<RunSummary> RunTestCase(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var preInvokeFailed = true;

		if (exception is null)
		{
			try
			{
				ctxt.TestCase.PreInvoke();
				preInvokeFailed = false;
			}
			catch (Exception ex)
			{
				exception = ex;
			}
		}

		var result = await RunTestCaseInner(ctxt, exception);

		if (!preInvokeFailed)
			ctxt.Aggregator.Run(ctxt.TestCase.PostInvoke);

		return result;
	}

	/// <summary>
	/// Override this to run the test case.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test case</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected virtual async ValueTask<RunSummary> RunTestCaseInner(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (exception is not null || ctxt.ParallelMode != ParallelMode.All)
			return await base.RunTestCase(ctxt, exception);

		List<ValueTask<RunSummary>>? parallelTasks = null;
		List<Func<ValueTask<RunSummary>>>? nonParallelTaskFactories = null;
		var summary = new RunSummary();

		foreach (var test in ctxt.Tests)
		{
			ValueTask<RunSummary> taskFactory() => RunTest(ctxt, test);

			if (test.DisableParallelization)
				(nonParallelTaskFactories ??= []).Add(taskFactory);
			else
#pragma warning disable CA2012
				(parallelTasks ??= []).Add(taskFactory());
#pragma warning restore CA2012

			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				break;
		}

		if (parallelTasks?.Count > 0)
			foreach (var parallelTask in parallelTasks)
				try
				{
					summary.Aggregate(await parallelTask);
				}
				catch (TaskCanceledException) { }

		if (nonParallelTaskFactories?.Count > 0)
			foreach (var nonParallelTaskFactory in nonParallelTaskFactories)
				try
				{
					summary.Aggregate(await ctxt.Scheduler.RunSequentialTask(nonParallelTaskFactory, ctxt.CancellationTokenSource.Token));
				}
				catch (TaskCanceledException) { }

		return summary;
	}

	/// <summary>
	/// Runs the test via the context.
	/// </summary>
	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTest(
		TContext ctxt,
		TTest test) =>
			Guard.ArgumentNotNull(ctxt).RunTest(test);
}
