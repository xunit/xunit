using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class CoreTestCaseRunnerTests
{
	public static class InvokeHandlers
	{
		[Fact]
		public static async ValueTask RunsPreAndPostInvokeByDefault()
		{
			var operations = new List<string>();
			var testCase = Mocks.CoreTestCase(
				preInvoke: () => operations.Add("PreInvoke()"),
				postInvoke: () => operations.Add("PostInvoke()")
			);
			var runner = new TestableCoreTestCaseRunner([Mocks.CoreTest(testCase: testCase)]);

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
			var runner = new TestableCoreTestCaseRunner([Mocks.CoreTest(testCase: testCase)]);

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
			var runner = new TestableCoreTestCaseRunner([Mocks.CoreTest(testCase: testCase)]);
			runner.Aggregator.Add(new DivideByZeroException());

			var result = await runner.RunAsync();

			Assert.Equal(1, result.Total);
			Assert.Equal(1, result.Failed);
			Assert.Equal(0, result.Skipped);
			Assert.Equal(0, result.NotRun);
			Assert.Empty(operations);
		}
	}

	public class Parallelization
	{
		[Fact]
		public async ValueTask ParallelTests()
		{
			var testTcs1 = new TaskCompletionSource<bool>(TaskCreationOptions.None);
			var testTcs2 = new TaskCompletionSource<bool>(TaskCreationOptions.None);
			var testCase = Mocks.CoreTestCase(testCaseDisplayName: "TestCase1");
			var test1 = Mocks.CoreTest(testCase: testCase, testDisplayName: "Test1");
			var test2 = Mocks.CoreTest(testCase: testCase, testDisplayName: "Test2");

			var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
			var completionTask = Task.WhenAny(timeoutTask, Task.WhenAll(testTcs1.Task, testTcs2.Task));
			var runner = new TestableCoreTestCaseRunner([test1, test2], runTest, parallelismOptions: ParallelismOptions.All);

			await runner.RunAsync();

			Assert.False(timeoutTask.IsCompleted, "Timed out waiting for tests to run in parallel.");
			async ValueTask<RunSummary> runTest(ICoreTest test)
			{
				if (test == test1)
				{
					testTcs1.TrySetResult(true);
				}
				else
				{
					testTcs2.TrySetResult(true);
				}

				await completionTask;
				return new RunSummary { Total = 1 };
			}
		}

		[Fact]
		public async ValueTask SerialTests()
		{
			var messages = new List<string>();
			var testCase = Mocks.CoreTestCase(testCaseDisplayName: "TestCase1");
			var test1 = Mocks.CoreTest(testCase: testCase, testDisplayName: "Test1");
			var test2 = Mocks.CoreTest(testCase: testCase, testDisplayName: "Test2");

			var runner = new TestableCoreTestCaseRunner([test1, test2], runTestLamda: runTest);

			await runner.RunAsync();

			// let each test finish before the next one runs, despite sleeping. However, we don't know which one
			// gets to go first, so we look at the first one to see which one it is, and make sure the post-sleep happens
			// directly after the pre-sleep
			var firstMessage = messages[0];
			Assert.Contains("pre-sleep", firstMessage);
			Assert.Equal(firstMessage.Replace("pre-sleep", "post-sleep"), messages[1]);

			var thirdMessage = messages[2];
			Assert.NotEqual(firstMessage, thirdMessage);
			Assert.Contains("pre-sleep", thirdMessage);
			Assert.Equal(thirdMessage.Replace("pre-sleep", "post-sleep"), messages[3]);

			async ValueTask<RunSummary> runTest(ICoreTest test)
			{
				messages.Add($"{test.TestDisplayName} pre-sleep");

				await Task.Delay(50, TestContext.Current.CancellationToken);

				messages.Add($"{test.TestDisplayName} post-sleep");

				return new RunSummary { Total = 1 };
			}
		}

		[Fact]
		public async ValueTask ParallelTestsWithSemaphore()
		{
			var messages = new List<string>();
			var testCase = Mocks.CoreTestCase(testCaseDisplayName: "TestCase1");
			var test1 = Mocks.CoreTest(testCase: testCase, testDisplayName: "Test1");
			var test2 = Mocks.CoreTest(testCase: testCase, testDisplayName: "Test2");
			var runner = new TestableCoreTestCaseRunner([test1, test2], runTest,
				parallelismOptions: ParallelismOptions.All, parallelizationSemaphore: new TestPipelineSemaphore(maximumConcurrentTests: 1));

			await runner.RunAsync();

			// let each test finish before the next one runs, despite sleeping. However, we don't know which one
			// gets to go first, so we look at the first one to see which one it is, and make sure the post-sleep happens
			// directly after the pre-sleep
			var firstMessage = messages[0];
			Assert.Contains("pre-sleep", firstMessage);
			Assert.Equal(firstMessage.Replace("pre-sleep", "post-sleep"), messages[1]);

			var thirdMessage = messages[2];
			Assert.NotEqual(firstMessage, thirdMessage);
			Assert.Contains("pre-sleep", thirdMessage);
			Assert.Equal(thirdMessage.Replace("pre-sleep", "post-sleep"), messages[3]);

			async ValueTask<RunSummary> runTest(ICoreTest test)
			{
				messages.Add($"{test.TestDisplayName} pre-sleep");

				await Task.Delay(50, TestContext.Current.CancellationToken);

				messages.Add($"{test.TestDisplayName} post-sleep");

				return new RunSummary { Total = 1 };
			}
		}
	}

	class TestableCoreTestCaseRunner(
		ICoreTest[] tests,
		Func<ICoreTest, ValueTask<RunSummary>>? runTestLamda = null,
		ParallelismOptions parallelismOptions = ParallelismOptionsAliases.Default,
		ITestPipelineSemaphore? parallelizationSemaphore = null) :
		CoreTestCaseRunner<TestableCoreTestCaseRunner.TestableContext, ICoreTestCase, ICoreTest>
	{
		public readonly ExceptionAggregator Aggregator = new();
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public readonly SpyMessageBus MessageBus = new();

		public async ValueTask<RunSummary> RunAsync()
		{
			var testCase = tests[0].TestCase;
			await using var ctxt = new TestableContext(
				testCase,
				tests,
				ExplicitOption.Off,
				MessageBus,
				Aggregator,
				testCase.TestCaseDisplayName,
				testCase.SkipReason,
				parallelismOptions,
				parallelizationSemaphore,
				runTestLamda ?? (_ => new ValueTask<RunSummary>(new RunSummary { Total = 1 })),
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
			ParallelismOptions parallelismOptions,
			ITestPipelineSemaphore? parallelizationSemaphore,
			Func<ICoreTest, ValueTask<RunSummary>> runTestLambda,
			CancellationTokenSource cancellationTokenSource) :
			CoreTestCaseRunnerContext<ICoreTestCase, ICoreTest>(testCase, tests, explicitOption, messageBus, aggregator,
				displayName, skipReason, parallelismOptions,
				parallelizationSemaphore, cancellationTokenSource)
		{
			public override ValueTask<RunSummary> RunTest(ICoreTest test) => runTestLambda(test);
		}
	}
}
