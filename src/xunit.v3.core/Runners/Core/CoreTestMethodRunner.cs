using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base test method runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is shared between reflection-based and code generation-based tests.
/// </remarks>
public class CoreTestMethodRunner<TContext, TTestMethod, TTestCase> : TestMethodRunner<TContext, TTestMethod, TTestCase>
	where TContext : CoreTestMethodRunnerContext<TTestMethod, TTestCase>
	where TTestMethod : class, ICoreTestMethod
	where TTestCase : class, ICoreTestCase
{
	/// <summary>
	/// Orders the test cases using the first available orderer from:
	/// <list type="bullet">
	/// <item><see cref="ICoreTestMethod.TestCaseOrderer"/></item>
	/// <item><see cref="ICoreTestClass.TestCaseOrderer"/></item>
	/// <item><see cref="ICoreTestCollection.TestCaseOrderer"/></item>
	/// <item><see cref="ICoreTestAssembly.TestCaseOrderer"/></item>
	/// <item><see cref="DefaultTestClassOrderer"/></item>
	/// </list>
	/// </summary>
	/// <inheritdoc/>
	protected override IReadOnlyCollection<TTestCase> OrderTestCases(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var testCaseOrderer =
			ctxt.TestMethod.TestCaseOrderer
				?? ctxt.TestMethod.TestClass.TestCaseOrderer
				?? ctxt.TestMethod.TestClass.TestCollection.TestCaseOrderer
				?? ctxt.TestMethod.TestClass.TestCollection.TestAssembly.TestCaseOrderer
				?? DefaultTestCaseOrderer.Instance;

		try
		{
			return testCaseOrderer.OrderTestCases(ctxt.TestCases);
		}
		catch (Exception ex)
		{
			var innerEx = ex.Unwrap();

			throw new TestPipelineException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Test case orderer '{0}' threw during ordering",
					testCaseOrderer.GetType().SafeName()
				),
				innerEx
			);
		}
	}

	/// <summary>
	/// Runs the list of test cases. It runs the cases in order serially, or in parallel
	/// if <see cref="ParallelismOptions.TestCases"/> is set.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test method cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	[SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly",
		Justification = "We guarantee that parallel ValueTasks are only awaited once.")]
	protected override async ValueTask<RunSummary> RunTestCases(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (!ctxt.ParallelismOptions.HasFlag(ParallelismOptions.TestCases))
		{
			using var _ = ctxt.ParallelizationSemaphore != null
				? await ctxt.ParallelizationSemaphore.LockAsync(ctxt.CancellationTokenSource.Token)
				: null;

			return await base.RunTestCases(ctxt, exception);
		}

		var summary = new RunSummary();
		var orderedTestCases = OrderTestCases(ctxt);
		var taskRunner = TestPipelineTaskRunner.Create(ctxt.CancellationTokenSource.Token);
		List<ValueTask<RunSummary>> parallel = [];

		foreach (var testCase in orderedTestCases)
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
				return await (exception == null
					? RunTestCase(ctxt, testCase)
					: FailTestCase(ctxt, testCase, exception));
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

		return summary;
	}

	/// <summary>
	/// Runs the test case via the context.
	/// </summary>
	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestCase(
		TContext ctxt,
		TTestCase testCase) =>
			Guard.ArgumentNotNull(ctxt).RunTestCase(testCase);
}
