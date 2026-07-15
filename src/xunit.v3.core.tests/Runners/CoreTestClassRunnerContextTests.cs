using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class CoreTestClassRunnerContextTests
{
	[Theory]
	[InlineData(ParallelMode.All, false, ParallelMode.All)]
	[InlineData(ParallelMode.All, true, ParallelMode.None)]
	[InlineData(ParallelMode.None, false, ParallelMode.None)]
	[InlineData(ParallelMode.None, true, ParallelMode.None)]
	public static async ValueTask ParallelModeHandling(
		ParallelMode parallelMode,
		bool disableParallelization,
		ParallelMode expectedParallelMode)
	{
		var testClass = Mocks.CoreTestClass(disableParallelization: disableParallelization);

		var context = TestableCoreTestClassRunnerContext.Create(testClass, parallelMode);

		Assert.Equal(expectedParallelMode, context.ParallelMode);
	}

	class TestableCoreTestClassRunnerContext(
		ICoreTestClass testClass,
		IReadOnlyCollection<ICoreTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		ParallelMode parallelMode,
		ExecutionScheduler scheduler) :
			CoreTestClassRunnerContext<ICoreTestClass, ICoreTestMethod, ICoreTestCase>(testClass, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler)
	{
		public override ValueTask<RunSummary> RunTestMethod(
			ICoreTestMethod testMethod,
			IReadOnlyCollection<ICoreTestCase> testCases) =>
				new(new RunSummary());

		public static TestableCoreTestClassRunnerContext Create(
			ICoreTestClass testClass,
			ParallelMode parallelMode)
		{
			return new(
				testClass,
				[Mocks.CoreTestCase(testMethod: Mocks.CoreTestMethod(testClass: testClass))],
				ExplicitOption.Off,
				new SpyMessageBus(),
				new(),
				new(),
				parallelMode,
				new SpyExecutionScheduler()
			);
		}
	}
}
