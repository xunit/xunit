using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class CoreTestCollectionRunnerTests
{
	public class Run
	{
		public static TheoryData<ICoreTestCollection> TestClassOrdererData = new()
		{
			// Assembly level orderer
			Mocks.CoreTestCollection(testAssembly: Mocks.CoreTestAssembly(testClassOrderer: UnorderedTestClassOrderer.Instance)),
			// Collection level orderer
			Mocks.CoreTestCollection(testClassOrderer: UnorderedTestClassOrderer.Instance),
		};

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(TestClassOrdererData))]
		public static async ValueTask OrdersTestClasses(ICoreTestCollection testCollection)
		{
			var testClass1 = Mocks.CoreTestClass(testCollection: testCollection, testClassName: "test-class-1", uniqueID: "1");
			var testCase1 = testCaseForClass(testClass1, "test-case-1");
			var testClass2 = Mocks.CoreTestClass(testCollection: testCollection, testClassName: "test-class-2", uniqueID: "2");
			var testCase2 = testCaseForClass(testClass2, "test-case-2");
			var testClass3 = Mocks.CoreTestClass(testCollection: testCollection, testClassName: "test-class-3", uniqueID: "3");
			var testCase3 = testCaseForClass(testClass3, "test-case-3");
			var runner = new TestableCoreTestCollectionRunner(testCase3, testCase1, testCase2);

			await runner.RunAsync();

			Assert.Collection(
				runner.TestClassesRun,
				tc =>
				{
					Assert.Equal("test-class-3", tc.TestClass.TestClassName);
					Assert.Equal(["test-case-3"], tc.TestCases.Select(tc => tc.TestCaseDisplayName));
				},
				tc =>
				{
					Assert.Equal("test-class-1", tc.TestClass.TestClassName);
					Assert.Equal(["test-case-1"], tc.TestCases.Select(tc => tc.TestCaseDisplayName));
				},
				tc =>
				{
					Assert.Equal("test-class-2", tc.TestClass.TestClassName);
					Assert.Equal(["test-case-2"], tc.TestCases.Select(tc => tc.TestCaseDisplayName));
				}
			);

			static ICoreTestCase testCaseForClass(
				ICoreTestClass testClass,
				string testCaseDisplayName) =>
					Mocks.CoreTestCase(testMethod: Mocks.CoreTestMethod(testClass: testClass), testCaseDisplayName: testCaseDisplayName);
		}

		[Fact]
		public async ValueTask ThrowingOrderer()
		{
			var testCollection = Mocks.CoreTestCollection(testClassOrderer: new MyThrowingOrderer());
			var testClass = Mocks.CoreTestClass(testCollection: testCollection);
			var testMethod = Mocks.CoreTestMethod(testClass: testClass);
			var testCase = Mocks.CoreTestCase(testMethod: testMethod);
			var runner = new TestableCoreTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg =>
				{
					var failure = Assert.IsType<ITestCollectionCleanupFailure>(msg, exactMatch: false);
					Assert.Collection(
						failure.ExceptionTypes,
						type => Assert.Equal(typeof(TestPipelineException).SafeName(), type),
						type => Assert.Equal(typeof(DivideByZeroException).SafeName(), type)
					);
					Assert.Collection(
						failure.Messages,
						msg => Assert.Equal($"Test class orderer '{typeof(MyThrowingOrderer).FullName}' threw during ordering", msg),
						msg => Assert.Equal("Attempted to divide by zero.", msg)
					);
				},
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
			);
		}

		class MyThrowingOrderer : ITestClassOrderer
		{
			public IReadOnlyCollection<TTestClass?> OrderTestClasses<TTestClass>(IReadOnlyCollection<TTestClass?> testClasses)
				where TTestClass : notnull, ITestClass =>
					throw new DivideByZeroException();
		}
	}

	class TestableCoreTestCollectionRunner(params ICoreTestCase[] testCases) :
		CoreTestCollectionRunner<TestableCoreTestCollectionRunner.TestableContext, ICoreTestCollection, ICoreTestClass, ICoreTestCase>
	{
		public ExceptionAggregator Aggregator = new();
		public CancellationTokenSource CancellationTokenSource = new();
		public SpyMessageBus MessageBus = new();
		public List<(ICoreTestClass TestClass, IReadOnlyCollection<ICoreTestCase> TestCases)> TestClassesRun = [];

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var context = new TestableContext(
				testCases[0].TestCollection,
				testCases,
				ExplicitOption.Off,
				MessageBus,
				Aggregator,
				CancellationTokenSource
			);
			await context.InitializeAsync();

			return await Run(context);
		}

		protected override ValueTask<RunSummary> RunTestClass(
			TestableContext ctxt,
			ICoreTestClass? testClass,
			IReadOnlyCollection<ICoreTestCase> testCases)
		{
			TestClassesRun.Add((testClass!, testCases));

			return base.RunTestClass(ctxt, testClass, testCases);
		}

		public class TestableContext(
			ICoreTestCollection testCollection,
			IReadOnlyCollection<ICoreTestCase> testCases,
			ExplicitOption explicitOption,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource) :
				CoreTestCollectionRunnerContext<ICoreTestCollection, ICoreTestClass, ICoreTestCase>(testCollection, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
		{
			public override ValueTask<RunSummary> RunTestClass(
				ICoreTestClass testClass,
				IReadOnlyCollection<ICoreTestCase> testCases) =>
					new(new RunSummary());
		}
	}
}
