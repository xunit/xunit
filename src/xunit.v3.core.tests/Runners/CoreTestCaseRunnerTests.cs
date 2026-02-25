using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class CoreTestCaseRunnerTests
{
	public class InvokeHandlers
	{
		[Fact]
		public async ValueTask RunsPreAndPostInvokeByDefault()
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
		public async ValueTask PreInvokeFails_SkipsPostInvoke()
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
		public async ValueTask AggregatorContainsException_SkipsPreAndPostInvoke()
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
	}

	class TestableCoreTestCaseRunner(ICoreTestCase testCase) :
		CoreTestCaseRunner<TestableCoreTestCaseRunner.TestableContext, ICoreTestCase, ICoreTest>
	{
		public readonly ExceptionAggregator Aggregator = new();
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public readonly SpyMessageBus MessageBus = new();

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new TestableContext(
				testCase,
				[Mocks.CoreTest(testCase: testCase)],
				ExplicitOption.Off,
				MessageBus,
				Aggregator,
				testCase.TestCaseDisplayName,
				testCase.SkipReason,
				CancellationTokenSource
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
			CancellationTokenSource cancellationTokenSource) :
				CoreTestCaseRunnerContext<ICoreTestCase, ICoreTest>(testCase, tests, explicitOption, messageBus, aggregator, displayName, skipReason, cancellationTokenSource)
		{
			public override ValueTask<RunSummary> RunTest(ICoreTest test) =>
				new(new RunSummary { Total = 1 });
		}
	}
}
