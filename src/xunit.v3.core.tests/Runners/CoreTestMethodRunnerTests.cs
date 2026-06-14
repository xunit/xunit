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
			var runner = new TestableCoreTestMethodRunner(testCase3, testCase1, testCase2);

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
			var runner = new TestableCoreTestMethodRunner(testCase);

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

		class MyThrowingOrderer : ITestCaseOrderer
		{
			public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
				where TTestCase : notnull, ITestCase =>
					throw new DivideByZeroException();
		}

		[Theory]
		[InlineData(ParallelMode.All, true, "RunSequentialTask<RunSummary>")]  // All: Opting out moves us through the sequential gate
		[InlineData(ParallelMode.All, false)]                                  // All: Not opting out does noting
		[InlineData(ParallelMode.None, true)]                                  // None: Do nothing
		[InlineData(ParallelMode.None, false)]                                 // None: Do nothing
		public static async ValueTask ParallelModeHandling(
			ParallelMode testMethodParallelMode,
			bool testCaseDisableParallelization,
			string? expectedOperation = null)
		{
			var spyScheduler = new SpyExecutionScheduler();
			var testCase = Mocks.CoreTestCase(disableParallelization: testCaseDisableParallelization);
			var runner = new TestableCoreTestMethodRunner(testCase) { ParallelMode = testMethodParallelMode, Scheduler = spyScheduler };

			await runner.RunAsync();

			if (expectedOperation is null)
				Assert.Empty(spyScheduler.Operations);
			else
				Assert.Equal(expectedOperation, Assert.Single(spyScheduler.Operations));
		}
	}

	class TestableCoreTestMethodRunner(params ICoreTestCase[] testCases) :
		CoreTestMethodRunner<TestableCoreTestMethodRunner.TestableContext, ICoreTestMethod, ICoreTestCase>
	{
		public ExceptionAggregator Aggregator = new();
		public CancellationTokenSource CancellationTokenSource = new();
		public SpyMessageBus MessageBus = new();
		public ParallelMode ParallelMode = ParallelMode.Collections;
		public ExecutionScheduler Scheduler = ExecutionScheduler.CreateUnlimited();
		public List<ICoreTestCase> TestCasesRun = [];

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var context = new TestableContext(
				testCases[0].TestMethod,
				testCases,
				ExplicitOption.Off,
				MessageBus,
				Aggregator,
				CancellationTokenSource,
				ParallelMode,
				Scheduler
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
			CancellationTokenSource cancellationTokenSource,
			ParallelMode parallelMode,
			ExecutionScheduler scheduler) :
				CoreTestMethodRunnerContext<ICoreTestMethod, ICoreTestCase>(testMethod, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler)
		{
			public override ValueTask<RunSummary> RunTestCase(ICoreTestCase testCase) =>
				new(new RunSummary());
		}
	}
}
