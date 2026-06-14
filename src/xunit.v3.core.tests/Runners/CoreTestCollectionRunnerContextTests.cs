using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class CoreTestCollectionRunnerContextTests
{
	[Theory]
	[InlineData(ParallelMode.All, false, ParallelMode.All)]
	[InlineData(ParallelMode.All, true, ParallelMode.None)]
	[InlineData(ParallelMode.Collections, false, ParallelMode.None)]
	[InlineData(ParallelMode.Collections, true, ParallelMode.None)]
	[InlineData(ParallelMode.None, false, ParallelMode.None)]
	[InlineData(ParallelMode.None, true, ParallelMode.None)]
	public static async ValueTask ParallelModeHandling(
		ParallelMode parallelMode,
		bool disableParallelization,
		ParallelMode expectedParallelMode)
	{
		var testCollection = Mocks.CoreTestCollection(disableParallelization: disableParallelization);

		var context = TestableCoreTestCollectionRunnerContext.Create(testCollection, parallelMode);

		Assert.Equal(expectedParallelMode, context.ParallelMode);
	}

	class TestableCoreTestCollectionRunnerContext(
		ICoreTestCollection testCollection,
		IReadOnlyCollection<ICoreTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		ParallelMode parallelMode,
		ExecutionScheduler scheduler) :
			CoreTestCollectionRunnerContext<ICoreTestCollection, ICoreTestClass, ICoreTestCase>(testCollection, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler)
	{
		public override ValueTask<RunSummary> RunTestClass(
			ICoreTestClass testClass,
			IReadOnlyCollection<ICoreTestCase> testCases) =>
				new(new RunSummary());

		public static TestableCoreTestCollectionRunnerContext Create(
			ICoreTestCollection testCollection,
			ParallelMode parallelMode)
		{
			return new(
				testCollection,
				[Mocks.CoreTestCase(testMethod: Mocks.CoreTestMethod(testClass: Mocks.CoreTestClass(testCollection: testCollection)))],
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
