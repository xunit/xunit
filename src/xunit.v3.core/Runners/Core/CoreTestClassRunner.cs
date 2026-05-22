using Xunit.Sdk;
using Xunit.v3.Utility;

namespace Xunit.v3;

/// <summary>
/// Base test class runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is shared between reflection-based and code generation-based tests.
/// </remarks>
public class CoreTestClassRunner<TContext, TTestClass, TTestMethod, TTestCase> : TestClassRunner<TContext, TTestClass, TTestMethod, TTestCase>
	where TContext : CoreTestClassRunnerContext<TTestClass, TTestMethod, TTestCase>
	where TTestClass : class, ICoreTestClass
	where TTestMethod : class, ICoreTestMethod
	where TTestCase : class, ICoreTestCase
{
	/// <summary>
	/// Orders the test methods using the first available orderer from:
	/// <list type="bullet">
	/// <item><see cref="ICoreTestClass.TestMethodOrderer"/></item>
	/// <item><see cref="ICoreTestCollection.TestMethodOrderer"/></item>
	/// <item><see cref="ICoreTestAssembly.TestMethodOrderer"/></item>
	/// <item><see cref="DefaultTestClassOrderer"/></item>
	/// </list>
	/// </summary>
	/// <inheritdoc/>
	protected override List<(TTestMethod? Method, List<TTestCase> TestCases)> OrderTestMethods(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var testCasesByMethod =
			ctxt.TestCases
				.GroupBy(tc => tc.TestMethod as TTestMethod, TestMethodComparer<TTestMethod>.Instance)
				.ToDictionary(group => new Maybe<TTestMethod>(group.Key), group => group.ToList());

		var testMethodOrderer =
			ctxt.TestClass.TestMethodOrderer
				?? ctxt.TestClass.TestCollection.TestMethodOrderer
				?? ctxt.TestClass.TestCollection.TestAssembly.TestMethodOrderer
				?? DefaultTestMethodOrderer.Instance;

		try
		{
			var orderedTestMethods = testMethodOrderer.OrderTestMethods(testCasesByMethod.Keys.Select(k => k.Value).CastOrToReadOnlyCollection());

			return
				orderedTestMethods
					.Select(testClass => (testClass, testCasesByMethod[testClass]))
					.ToList();
		}
		catch (Exception ex)
		{
			var innerEx = ex.Unwrap();

			throw new TestPipelineException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Test method orderer '{0}' threw during ordering",
					testMethodOrderer.GetType().SafeName()
				),
				innerEx
			);
		}
	}

	/// <summary>
	/// Runs the test method via the context (after validating that it's not <see langword="null"/>).
	/// </summary>
	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestMethod(
		TContext ctxt,
		TTestMethod? testMethod,
		IReadOnlyCollection<TTestCase> testCases)
	{
		Guard.ArgumentNotNull(ctxt);

		// Technically not possible because ICoreTestCase always comes from a method, but this signature is imposed
		// by the base class, which allows method-less tests.
		if (testMethod is null)
			return new(XunitRunnerHelper.FailTestCases(
				ctxt.MessageBus,
				ctxt.CancellationTokenSource,
				testCases,
				"Test case '{0}' does not have an associated method and cannot be run by XunitTestMethodRunner",
				sendTestMethodMessages: true
			));

		return ctxt.RunTestMethod(testMethod, testCases);
	}

	/// <summary>
	/// Runs the list of test methods. Orders the tests, groups them by method,
	/// and runs them in parallel if <see cref="ParallelismOptions.Methods"/> is set, and serially otherwise.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test class cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run</returns>
	[SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly",
		Justification = "We guarantee that parallel ValueTasks are only awaited once.")]
	protected override async ValueTask<RunSummary> RunTestMethods(TContext ctxt, Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (!ctxt.ParallelismOptions.HasFlag(ParallelismOptions.Methods))
		{
			return await base.RunTestMethods(ctxt, exception);
		}

		var summary = new RunSummary();
		var taskRunner = TestPipelineTaskRunner.Create(ctxt.CancellationTokenSource.Token);
		List<ValueTask<RunSummary>> parallel = [];

		var orderedTestMethods = exception is null
			? OrderTestMethods(ctxt)
			: OrderTestMethodsDefault(ctxt);

		foreach (var testMethod in orderedTestMethods)
		{
			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				break;

			parallel.Add(taskRunner(task));

			ValueTask<RunSummary> task() =>
				exception == null
					? RunTestMethod(ctxt, testMethod.Method, testMethod.TestCases)
					: FailTestMethod(ctxt, testMethod.Method, testMethod.TestCases, exception);
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
}
