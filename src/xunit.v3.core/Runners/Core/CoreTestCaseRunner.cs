using Xunit.Sdk;
using Xunit.v3.Utility;

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
	/// <inheritdoc/>
	[SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly",
		Justification = "We guarantee that parallel ValueTasks are only awaited once.")]
	protected override async ValueTask<RunSummary> RunTestCase(
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

		var summary = new RunSummary();
		if (ctxt.ParallelismOptions.HasFlag(ParallelismOptions.Tests))
		{
			var taskRunner = TestPipelineTaskRunner.Create(ctxt.CancellationTokenSource.Token);
			List<ValueTask<RunSummary>> parallel = [];

			foreach (var test in ctxt.Tests)
			{
				if (ctxt.CancellationTokenSource.IsCancellationRequested)
					break;

				parallel.Add(taskRunner(task));

				ValueTask<RunSummary> task() => exception is null
					? RunTest(ctxt, test)
					: FailTest(ctxt, test, exception);
			}

			foreach (var task in parallel)
			{
				try
				{
					summary.Aggregate(await task);
				}
				catch (TaskCanceledException)
				{
				}
			}
		}
		else
		{
			summary = await base.RunTestCase(ctxt, exception);
		}

		if (!preInvokeFailed)
			ctxt.Aggregator.Run(ctxt.TestCase.PostInvoke);

		return summary;
	}

	/// <summary>
	/// Runs the test via the context.
	/// </summary>
	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTest(
		TContext ctxt,
		TTest test)
	{
		Guard.ArgumentNotNull(ctxt);

		if (ctxt.ParallelizationSemaphore != null && !ctxt.ParallelismOptions.RunsTestsWithinCollectionSerially())
		{
			// acquire parallelization semaphore at the test level when running the collection's tests in parallel
			await ctxt.ParallelizationSemaphore.WaitAsync(ctxt.CancellationTokenSource.Token);
		}

		try
		{
			return await ctxt.RunTest(test);
		}
		finally
		{
			if (!ctxt.ParallelismOptions.RunsTestsWithinCollectionSerially())
			{
				ctxt.ParallelizationSemaphore?.Release();
			}
		}
	}
}
