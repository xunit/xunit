using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class CoreTestCaseRunnerContextTests
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
		var testCase = Mocks.CoreTestCase(disableParallelization: disableParallelization);

		var context = TestableCoreTestCaseRunnerContext.Create(testCase, parallelMode);

		Assert.Equal(expectedParallelMode, context.ParallelMode);
	}

	class TestableCoreTestCaseRunnerContext(
		ICoreTestCase testCase,
		IReadOnlyCollection<ICoreTest> tests,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		string displayName,
		string? skipReason,
		CancellationTokenSource cancellationTokenSource,
		ParallelMode parallelMode,
		ExecutionScheduler scheduler) :
			CoreTestCaseRunnerContext<ICoreTestCase, ICoreTest>(testCase, tests, explicitOption, messageBus, aggregator, displayName, skipReason, cancellationTokenSource, parallelMode, scheduler)
	{
		public override ValueTask<RunSummary> RunTest(ICoreTest test) =>
			new(new RunSummary());

		public static TestableCoreTestCaseRunnerContext Create(
			ICoreTestCase testCase,
			ParallelMode parallelMode)
		{
			return new(
				testCase,
				[Mocks.CoreTest(testCase: testCase)],
				ExplicitOption.Off,
				new SpyMessageBus(),
				new(),
				TestData.DefaultTestCaseDisplayName,
				skipReason: null,
				new(),
				parallelMode,
				new SpyExecutionScheduler()
			);
		}
	}
}
