using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class CoreTestMethodRunnerTests
{
	public static class Run
	{
		public static TheoryData<ICoreTestMethod> TestMethodOrdererData = new()
		{
			// Assembly level orderer
			Mocks.CoreTestMethod(testClass: Mocks.CoreTestClass(testCollection: Mocks.CoreTestCollection(testAssembly: Mocks.CoreTestAssembly(testCaseOrderer: UnorderedTestCaseOrderer.Instance)))),
			// Collection level orderer
			Mocks.CoreTestMethod(testClass: Mocks.CoreTestClass(testCollection: Mocks.CoreTestCollection(testCaseOrderer: UnorderedTestCaseOrderer.Instance))),
			// Class level orderer
			Mocks.CoreTestMethod(testClass: Mocks.CoreTestClass(testCaseOrderer: UnorderedTestCaseOrderer.Instance)),
			// Method level orderer
			Mocks.CoreTestMethod(testCaseOrderer: UnorderedTestCaseOrderer.Instance)
		};

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(TestMethodOrdererData))]
		public static async ValueTask OrdersTestCases(ICoreTestMethod testMethod)
		{
			var testCase1 = Mocks.CoreTestCase(testMethod: testMethod, testCaseDisplayName: "test-case-1");
			var testCase2 = Mocks.CoreTestCase(testMethod: testMethod, testCaseDisplayName: "test-case-2");
			var testCase3 = Mocks.CoreTestCase(testMethod: testMethod, testCaseDisplayName: "test-case-3");
			var runner = new TestableCoreTestMethodRunner([testCase3, testCase1, testCase2]);

			await runner.RunAsync();

			Assert.Collection(
				runner.TestCasesRun,
				tc => Assert.Equal("test-case-3", tc.TestCaseDisplayName),
				tc => Assert.Equal("test-case-1", tc.TestCaseDisplayName),
				tc => Assert.Equal("test-case-2", tc.TestCaseDisplayName)
			);
		}

		[Fact]
		public static async ValueTask ThrowingOrderer()
		{
			var testMethod = Mocks.CoreTestMethod(testCaseOrderer: new MyThrowingOrderer());
			var testCase = Mocks.CoreTestCase(testMethod: testMethod);
			var runner = new TestableCoreTestMethodRunner([testCase]);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg =>
				{
					var failure = Assert.IsType<ITestMethodCleanupFailure>(msg, exactMatch: false);
					Assert.Collection(
						failure.ExceptionTypes,
						type => Assert.Equal(typeof(TestPipelineException).SafeName(), type),
						type => Assert.Equal(typeof(DivideByZeroException).SafeName(), type)
					);
					Assert.Collection(
						failure.Messages,
						msg => Assert.Equal($"Test case orderer '{typeof(MyThrowingOrderer).FullName}' threw during ordering", msg),
						msg => Assert.Equal("Attempted to divide by zero.", msg)
					);
				},
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask ParallelTestCases()
		{
			var testCaseTcs1 = new TaskCompletionSource<bool>(TaskCreationOptions.None);
			var testCaseTcs2 = new TaskCompletionSource<bool>(TaskCreationOptions.None);

			var testMethod = Mocks.CoreTestMethod();
			var testCase1 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase1", testMethod: testMethod);
			var testCase2 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase2", testMethod: testMethod);

			var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
			var completionTask = Task.WhenAny(timeoutTask, Task.WhenAll(testCaseTcs1.Task, testCaseTcs2.Task));
			var runner = new TestableCoreTestMethodRunner([testCase1, testCase2], runTestCase, parallelismOptions: ParallelismOptions.All);

			await runner.RunAsync();

			Assert.False(timeoutTask.IsCompleted, "Timed out waiting for test cases to run in parallel.");
			async ValueTask<RunSummary> runTestCase(ICoreTestCase test)
			{
				if (test == testCase1)
				{
					testCaseTcs1.TrySetResult(true);
				}
				else
				{
					testCaseTcs2.TrySetResult(true);
				}

				await completionTask;
				return new RunSummary { Total = 1 };
			}
		}

		[Fact]
		public static async ValueTask SerialTestCases()
		{
			var messages = new List<string>();
			var testMethod = Mocks.CoreTestMethod();
			var testCase1 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase1", testMethod: testMethod);
			var testCase2 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase2", testMethod: testMethod);

			var runner = new TestableCoreTestMethodRunner([testCase1, testCase2], runTestCaseLamda: runTestCase);

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

			async ValueTask<RunSummary> runTestCase(ICoreTestCase testCase)
			{
				messages.Add($"{testCase.TestCaseDisplayName} pre-sleep");

				await Task.Delay(50, TestContext.Current.CancellationToken);

				messages.Add($"{testCase.TestCaseDisplayName} post-sleep");

				return new RunSummary { Total = 1 };
			}
		}

		class MyThrowingOrderer : ITestCaseOrderer
		{
			public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
				where TTestCase : notnull, ITestCase =>
					throw new DivideByZeroException();
		}
	}

	class TestableCoreTestMethodRunner(ICoreTestCase[] testCases, Func<ICoreTestCase, ValueTask<RunSummary>>? runTestCaseLamda = null, ParallelismOptions parallelismOptions = ParallelismOptionsAliases.Default) :
		CoreTestMethodRunner<TestableCoreTestMethodRunner.TestableContext, ICoreTestMethod, ICoreTestCase>
	{
		public ExceptionAggregator Aggregator = new();
		public CancellationTokenSource CancellationTokenSource = new();
		public SpyMessageBus MessageBus = new();
		public List<ICoreTestCase> TestCasesRun = [];

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var context = new TestableContext(
				testCases[0].TestMethod,
				testCases,
				ExplicitOption.Off,
				MessageBus,
				Aggregator,
				parallelismOptions,
				runTestCaseLamda ?? (_ => new(new RunSummary { Total = 1, })),
				CancellationTokenSource
			);
			await context.InitializeAsync();

			return await Run(context);
		}

		protected override ValueTask<RunSummary> RunTestCase(
			TestableContext ctxt,
			ICoreTestCase testCase)
		{
			TestCasesRun.Add(testCase);

			return base.RunTestCase(ctxt, testCase);
		}

		public class TestableContext(
			ICoreTestMethod testMethod,
			IReadOnlyCollection<ICoreTestCase> testCases,
			ExplicitOption explicitOption,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			ParallelismOptions parallelismOptions,
			Func<ICoreTestCase, ValueTask<RunSummary>> runTestCaseLamda,
			CancellationTokenSource cancellationTokenSource) :
				CoreTestMethodRunnerContext<ICoreTestMethod, ICoreTestCase>(
					testMethod,
					testCases, explicitOption, messageBus, aggregator, parallelismOptions, parallelizationSemaphore: null, cancellationTokenSource)
		{
			public override ValueTask<RunSummary> RunTestCase(ICoreTestCase testCase) => runTestCaseLamda(testCase);
		}
	}
}
