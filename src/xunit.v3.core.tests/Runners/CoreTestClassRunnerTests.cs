using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class CoreTestClassRunnerTests
{
	public class Run
	{
		public static TheoryData<ICoreTestClass> TestClassOrdererData = new()
		{
			// Assembly level orderer
			Mocks.CoreTestClass(testCollection: Mocks.CoreTestCollection(testAssembly: Mocks.CoreTestAssembly(testMethodOrderer: UnorderedTestMethodOrderer.Instance))),
			// Collection level orderer
			Mocks.CoreTestClass(testCollection: Mocks.CoreTestCollection(testMethodOrderer: UnorderedTestMethodOrderer.Instance)),
			// Class level orderer
			Mocks.CoreTestClass(testMethodOrderer: UnorderedTestMethodOrderer.Instance),
		};

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(TestClassOrdererData))]
		public static async ValueTask OrdersTestMethods(ICoreTestClass testClass)
		{
			var testMethod1 = Mocks.CoreTestMethod(testClass: testClass, methodName: "Method1", uniqueID: "1");
			var testCase1 = Mocks.CoreTestCase(testMethod: testMethod1, testCaseDisplayName: "test-case-1");
			var testMethod2 = Mocks.CoreTestMethod(testClass: testClass, methodName: "Method2", uniqueID: "2");
			var testCase2 = Mocks.CoreTestCase(testMethod: testMethod2, testCaseDisplayName: "test-case-2");
			var testMethod3 = Mocks.CoreTestMethod(testClass: testClass, methodName: "Method3", uniqueID: "3");
			var testCase3 = Mocks.CoreTestCase(testMethod: testMethod3, testCaseDisplayName: "test-case-3");
			var runner = new TestableCoreTestClassRunner(testCase3, testCase1, testCase2);

			await runner.RunAsync();

			Assert.Collection(
				runner.TestMethodsRun,
				tm =>
				{
					Assert.Equal("Method3", tm.TestMethod.MethodName);
					Assert.Equal(["test-case-3"], tm.TestCases.Select(tc => tc.TestCaseDisplayName));
				},
				tm =>
				{
					Assert.Equal("Method1", tm.TestMethod.MethodName);
					Assert.Equal(["test-case-1"], tm.TestCases.Select(tc => tc.TestCaseDisplayName));
				},
				tm =>
				{
					Assert.Equal("Method2", tm.TestMethod.MethodName);
					Assert.Equal(["test-case-2"], tm.TestCases.Select(tc => tc.TestCaseDisplayName));
				}
			);
		}

		[Fact]
		public async ValueTask ThrowingOrderer()
		{
			var testClass = Mocks.CoreTestClass(testMethodOrderer: new MyThrowingOrderer());
			var testMethod = Mocks.CoreTestMethod(testClass: testClass);
			var testCase = Mocks.CoreTestCase(testMethod: testMethod);
			var runner = new TestableCoreTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg =>
				{
					var failure = Assert.IsType<ITestClassCleanupFailure>(msg, exactMatch: false);
					Assert.Collection(
						failure.ExceptionTypes,
						type => Assert.Equal(typeof(TestPipelineException).SafeName(), type),
						type => Assert.Equal(typeof(DivideByZeroException).SafeName(), type)
					);
					Assert.Collection(
						failure.Messages,
						msg => Assert.Equal($"Test method orderer '{typeof(MyThrowingOrderer).FullName}' threw during ordering", msg),
						msg => Assert.Equal("Attempted to divide by zero.", msg)
					);
				},
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
			);
		}

		class MyThrowingOrderer : ITestMethodOrderer
		{
			public IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(IReadOnlyCollection<TTestMethod?> testMethods)
				where TTestMethod : notnull, ITestMethod =>
					throw new DivideByZeroException();
		}
	}

	class TestableCoreTestClassRunner(params ICoreTestCase[] testCases) :
		CoreTestClassRunner<TestableCoreTestClassRunner.TestableContext, ICoreTestClass, ICoreTestMethod, ICoreTestCase>
	{
		public ExceptionAggregator Aggregator = new();
		public CancellationTokenSource CancellationTokenSource = new();
		public SpyMessageBus MessageBus = new();
		public List<(ICoreTestMethod TestMethod, IReadOnlyCollection<ICoreTestCase> TestCases)> TestMethodsRun = [];

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var context = new TestableContext(
				testCases[0].TestClass,
				testCases,
				ExplicitOption.Off,
				MessageBus,
				Aggregator,
				CancellationTokenSource
			);
			await context.InitializeAsync();

			return await Run(context);
		}

		protected override ValueTask<RunSummary> RunTestMethod(
			TestableContext ctxt,
			ICoreTestMethod? testMethod,
			IReadOnlyCollection<ICoreTestCase> testCases)
		{
			TestMethodsRun.Add((testMethod!, testCases));

			return base.RunTestMethod(ctxt, testMethod, testCases);
		}

		public class TestableContext(
			ICoreTestClass testClass,
			IReadOnlyCollection<ICoreTestCase> testCases,
			ExplicitOption explicitOption,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource) :
				CoreTestClassRunnerContext<ICoreTestClass, ICoreTestMethod, ICoreTestCase>(testClass, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
		{
			public override ValueTask<RunSummary> RunTestMethod(
				ICoreTestMethod testMethod,
				IReadOnlyCollection<ICoreTestCase> testCases) =>
					new(new RunSummary());
		}
	}
}
