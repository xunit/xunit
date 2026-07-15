using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class CoreTestMethodRunnerContextTests
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
		var testMethod = Mocks.CoreTestMethod(disableParallelization: disableParallelization);

		var context = TestableCoreTestMethodRunnerContext.Create(testMethod, parallelMode);

		Assert.Equal(expectedParallelMode, context.ParallelMode);
	}

	class TestableCoreTestMethodRunnerContext(
		ICoreTestMethod testMethod,
		IReadOnlyCollection<ICoreTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		ParallelMode parallelMode,
		ExecutionScheduler scheduler) :
			CoreTestMethodRunnerContext<ICoreTestMethod, ICoreTestCase>(testMethod, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler)
	{
		public override ValueTask<RunSummary> RunTestCase(ICoreTestCase testCase) =>
			new(new RunSummary());

		public static TestableCoreTestMethodRunnerContext Create(
			ICoreTestMethod testMethod,
			ParallelMode parallelMode)
		{
			return new(
				testMethod,
				[Mocks.CoreTestCase(testMethod: testMethod)],
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
