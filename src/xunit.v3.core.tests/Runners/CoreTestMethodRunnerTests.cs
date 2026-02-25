using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class CoreTestMethodRunnerTests
{
	public class Run
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
		public async ValueTask ThrowingOrderer()
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
	}

	class TestableCoreTestMethodRunner(params ICoreTestCase[] testCases) :
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
			CancellationTokenSource cancellationTokenSource) :
				CoreTestMethodRunnerContext<ICoreTestMethod, ICoreTestCase>(testMethod, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
		{
			public override ValueTask<RunSummary> RunTestCase(ICoreTestCase testCase) =>
				new(new RunSummary());
		}
	}
}
