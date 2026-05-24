using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base test collection runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is shared between reflection-based and code generation-based tests.
/// </remarks>
public class CoreTestCollectionRunner<TContext, TTestCollection, TTestClass, TTestCase> :
	TestCollectionRunner<TContext, TTestCollection, TTestClass, TTestCase>
		where TContext : CoreTestCollectionRunnerContext<TTestCollection, TTestClass, TTestCase>
		where TTestCollection : class, ICoreTestCollection
		where TTestClass : class, ICoreTestClass
		where TTestCase : class, ICoreTestCase
{
	/// <summary>
	/// Orders the test classes using the first available orderer from:
	/// <list type="bullet">
	/// <item><see cref="ICoreTestCollection.TestClassOrderer"/></item>
	/// <item><see cref="ICoreTestAssembly.TestClassOrderer"/></item>
	/// <item><see cref="DefaultTestClassOrderer"/></item>
	/// </list>
	/// </summary>
	/// <inheritdoc/>
	protected override List<(TTestClass? Class, List<TTestCase> TestCases)> OrderTestClasses(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var testCasesByClass =
			ctxt.TestCases
				.GroupBy(tc => tc.TestClass as TTestClass, TestClassComparer<TTestClass>.Instance)
				.ToDictionary(group => new Maybe<TTestClass>(group.Key), group => group.ToList());

		var testClassOrderer =
			ctxt.TestCollection.TestClassOrderer
				?? ctxt.TestCollection.TestAssembly.TestClassOrderer
				?? DefaultTestClassOrderer.Instance;

		try
		{
			var orderedTestClasses = testClassOrderer.OrderTestClasses(testCasesByClass.Keys.Select(k => k.Value).CastOrToReadOnlyCollection());

			return
				orderedTestClasses
					.Select(testClass => (testClass, testCasesByClass[testClass]))
					.ToList();
		}
		catch (Exception ex)
		{
			var innerEx = ex.Unwrap();

			throw new TestPipelineException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Test class orderer '{0}' threw during ordering",
					testClassOrderer.GetType().SafeName()
				),
				innerEx
			);
		}
	}

	/// <summary>
	/// Runs the list of test classes. Groups the tests by class and runs them in parallel
	/// if <see cref="ParallelismOptions.Classes"/> is set, and serially otherwise.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test collection cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	[SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly",
		Justification = "We guarantee that parallel ValueTasks are only awaited once.")]
	protected override async ValueTask<RunSummary> RunTestClasses(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (!ctxt.ParallelismOptions.HasFlag(ParallelismOptions.Classes))
		{
			using var _ = ctxt.ParallelizationSemaphore != null
				? await ctxt.ParallelizationSemaphore.WaitAsync(ctxt.CancellationTokenSource.Token)
				: default;
			
			return await base.RunTestClasses(ctxt, exception);
		}

		var summary = new RunSummary();
		var orderedTestClasses = exception is null ? OrderTestClasses(ctxt) : OrderTestClassesDefault(ctxt);
		var taskRunner = TestPipelineTaskRunner.Create(ctxt.CancellationTokenSource.Token);

		List<ValueTask<RunSummary>> parallel = [];

		foreach (var testClass in orderedTestClasses)
		{
			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				break;

			var semaphoreReleaseHandle = ctxt.ParallelizationSemaphore != null
				? await ctxt.ParallelizationSemaphore.WaitAsync(ctxt.CancellationTokenSource.Token)
				: default;

			try
			{
				parallel.Add(taskRunner(task));
			}
			catch
			{
				semaphoreReleaseHandle.Dispose();
				throw;
			}

			async ValueTask<RunSummary> task()
			{
				using var _ = semaphoreReleaseHandle;
				return await (exception is null
					? RunTestClass(ctxt, testClass.Class, testClass.TestCases)
					: FailTestClass(ctxt, testClass.Class, testClass.TestCases, exception));
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
	/// Runs the test class via the context (after validating that it's not <see langword="null"/>).
	/// </summary>
	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestClass(
		TContext ctxt,
		TTestClass? testClass,
		IReadOnlyCollection<TTestCase> testCases)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(testCases);

		// Technically not possible because ICoreTestCase always comes from a class, but this signature is imposed
		// by the base class, which allows class-less tests.
		if (testClass is null)
			return new(XunitRunnerHelper.FailTestCases(
				ctxt.MessageBus,
				ctxt.CancellationTokenSource,
				testCases,
				$"Test case '{{0}}' does not have an associated class and cannot be run by '{GetType().Name}'",
				sendTestClassMessages: true,
				sendTestMethodMessages: true
			));

		return ctxt.RunTestClass(testClass, testCases);
	}
}
