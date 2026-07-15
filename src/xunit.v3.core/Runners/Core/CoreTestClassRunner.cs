using Xunit.Sdk;

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

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestMethods(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (exception is not null || ctxt.ParallelMode != ParallelMode.All)
			return await base.RunTestMethods(ctxt, exception);

		List<ValueTask<RunSummary>>? parallelTasks = null;
		List<Func<ValueTask<RunSummary>>>? nonParallelTaskFactories = null;
		var summary = new RunSummary();

		foreach (var (testMethod, testCases) in OrderTestMethods(ctxt))
		{
			ValueTask<RunSummary> taskFactory() => RunTestMethod(ctxt, testMethod, testCases);

			if (testMethod?.DisableParallelization == true)
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
}
