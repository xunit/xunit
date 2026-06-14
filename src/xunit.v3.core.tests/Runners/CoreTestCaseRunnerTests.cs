using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class CoreTestCaseRunnerTests
{
	public static class Run
	{
		[Fact]
		public static async ValueTask RunsPreAndPostInvokeByDefault()
		{
			var operations = new List<string>();
			var testCase = Mocks.CoreTestCase(
				preInvoke: () => operations.Add("PreInvoke()"),
				postInvoke: () => operations.Add("PostInvoke()")
			);
			var runner = new TestableCoreTestCaseRunner(testCase);

			var result = await runner.RunAsync();

			Assert.Equal(1, result.Total);
			Assert.Equal(0, result.Failed);
			Assert.Equal(0, result.Skipped);
			Assert.Equal(0, result.NotRun);
			Assert.Collection(
				operations,
				op => Assert.Equal("PreInvoke()", op),
				op => Assert.Equal("PostInvoke()", op)
			);
		}

		[Fact]
		public static async ValueTask PreInvokeFails_SkipsPostInvoke()
		{
			var operations = new List<string>();
			var testCase = Mocks.CoreTestCase(
				preInvoke: () => { operations.Add("PreInvoke()"); throw new DivideByZeroException(); },
				postInvoke: () => operations.Add("PostInvoke()")
			);
			var runner = new TestableCoreTestCaseRunner(testCase);

			var result = await runner.RunAsync();

			Assert.Equal(1, result.Total);
			Assert.Equal(1, result.Failed);
			Assert.Equal(0, result.Skipped);
			Assert.Equal(0, result.NotRun);
			Assert.Equal("PreInvoke()", Assert.Single(operations));
		}

		[Fact]
		public static async ValueTask AggregatorContainsException_SkipsPreAndPostInvoke()
		{
			var operations = new List<string>();
			var testCase = Mocks.CoreTestCase(
				preInvoke: () => operations.Add("PreInvoke()"),
				postInvoke: () => operations.Add("PostInvoke()")
			);
			var runner = new TestableCoreTestCaseRunner(testCase);
			runner.Aggregator.Add(new DivideByZeroException());

			var result = await runner.RunAsync();

			Assert.Equal(1, result.Total);
			Assert.Equal(1, result.Failed);
			Assert.Equal(0, result.Skipped);
			Assert.Equal(0, result.NotRun);
			Assert.Empty(operations);
		}

		[Theory]
		[InlineData(ParallelMode.All, true, "RunSequentialTask<RunSummary>")]  // All: Opting out moves us through the sequential gate
		[InlineData(ParallelMode.All, false)]                                  // All: Not opting out does noting
		[InlineData(ParallelMode.None, true)]                                  // None: Do nothing
		[InlineData(ParallelMode.None, false)]                                 // None: Do nothing
		public static async ValueTask ParallelModeHandling(
			ParallelMode testCaseParallelMode,
			bool testDisableParallelization,
			string? expectedOperation = null)
		{
			var spyScheduler = new SpyExecutionScheduler();
			var test = Mocks.CoreTest(disableParallelization: testDisableParallelization);
			var runner = new TestableCoreTestCaseRunner(test) { ParallelMode = testCaseParallelMode, Scheduler = spyScheduler };

			await runner.RunAsync();

			if (expectedOperation is null)
				Assert.Empty(spyScheduler.Operations);
			else
				Assert.Equal(expectedOperation, Assert.Single(spyScheduler.Operations));
		}
	}

	class TestableCoreTestCaseRunner : CoreTestCaseRunner<TestableCoreTestCaseRunner.TestableContext, ICoreTestCase, ICoreTest>
	{
		readonly ICoreTest test;

		public readonly ExceptionAggregator Aggregator = new();
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public readonly SpyMessageBus MessageBus = new();
		public ParallelMode ParallelMode = ParallelMode.Collections;
		public ExecutionScheduler Scheduler = ExecutionScheduler.CreateUnlimited();

		public TestableCoreTestCaseRunner(ICoreTest test) =>
			this.test = test;

		public TestableCoreTestCaseRunner(ICoreTestCase testCase) =>
			test = Mocks.CoreTest(testCase: testCase);

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new TestableContext(
				test.TestCase,
				[test],
				ExplicitOption.Off,
				MessageBus,
				Aggregator,
				test.TestCase.TestCaseDisplayName,
				test.TestCase.SkipReason,
				CancellationTokenSource,
				ParallelMode,
				Scheduler
			);
			await ctxt.InitializeAsync();

			return await Run(ctxt);
		}

		public class TestableContext(
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
				new(new RunSummary { Total = 1 });
		}
	}
}
