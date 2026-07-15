using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class CoreTestRunnerContextTests
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
		var testCase = Mocks.CoreTest(disableParallelization: disableParallelization);

		var context = TestableCoreTestRunnerContext.Create(testCase, parallelMode);

		Assert.Equal(expectedParallelMode, context.ParallelMode);
	}

	class TestableCoreTestRunnerContext(
		ICoreTest test,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		string? skipReason,
		CancellationTokenSource cancellationTokenSource,
		ParallelMode parallelMode,
		ExecutionScheduler scheduler) :
			CoreTestRunnerContext<ICoreTest, object>(test, explicitOption, messageBus, aggregator, skipReason, cancellationTokenSource, parallelMode, scheduler)
	{
		protected override IReadOnlyCollection<object> BeforeAfterTestAttributes { get => []; set { } }

		public override ValueTask<TimeSpan> InvokeTest(object? testClassInstance) =>
			new(TimeSpan.Zero);

		protected override string? GetRuntimeSkipReason() => null;

		public override void RunAfter(object attribute) { }

		public override void RunBefore(object attribute) { }

		public static TestableCoreTestRunnerContext Create(
			ICoreTest test,
			ParallelMode parallelMode)
		{
			return new(
				test,
				ExplicitOption.Off,
				new SpyMessageBus(),
				new(),
				skipReason: null,
				new(),
				parallelMode,
				new SpyExecutionScheduler()
			);
		}
	}
}
