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

				var semaphoreReleaser = ctxt.ParallelizationSemaphore != null
					? await ctxt.ParallelizationSemaphore.LockAsync(ctxt.CancellationTokenSource.Token)
					: null;

				try
				{
					parallel.Add(taskRunner(task));
				}
				catch
				{
					semaphoreReleaser?.Dispose();
					throw;
				}

				async ValueTask<RunSummary> task()
				{
					using var _ = semaphoreReleaser;
					return await (exception is null
						? RunTest(ctxt, test)
						: FailTest(ctxt, test, exception));
				}
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
			using var _ = ctxt.ParallelizationSemaphore != null
				? await ctxt.ParallelizationSemaphore.LockAsync(ctxt.CancellationTokenSource.Token)
				: null;

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
	protected override ValueTask<RunSummary> RunTest(
		TContext ctxt,
		TTest test) =>
		Guard.ArgumentNotNull(ctxt).RunTest(test);
}
