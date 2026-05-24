using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class CoreTestCollectionRunnerTests
{
	public static class Run
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
			var runner = new TestableCoreTestCollectionRunner([testCase3, testCase1, testCase2]);

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
		public static async ValueTask ThrowingOrderer()
		{
			var testCollection = Mocks.CoreTestCollection(testClassOrderer: new MyThrowingOrderer());
			var testClass = Mocks.CoreTestClass(testCollection: testCollection);
			var testMethod = Mocks.CoreTestMethod(testClass: testClass);
			var testCase = Mocks.CoreTestCase(testMethod: testMethod);
			var runner = new TestableCoreTestCollectionRunner([testCase]);

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

		[Fact]
		public static async ValueTask ParallelTestClasses()
		{
			var testClassTcs1 = new TaskCompletionSource<bool>(TaskCreationOptions.None);
			var testClassTcs2 = new TaskCompletionSource<bool>(TaskCreationOptions.None);

			var testCollection = Mocks.CoreTestCollection();
			var testClass1 = Mocks.CoreTestClass(testClassName: "TestClass1", testCollection: testCollection);
			var testClass2 = Mocks.CoreTestClass(testClassName: "TestClass2", testCollection: testCollection);
			var testMethod1 = Mocks.CoreTestMethod(methodName: "TestMethod1", testClass: testClass1);
			var testMethod2 = Mocks.CoreTestMethod(methodName: "TestMethod2", testClass: testClass2);
			var testCase1 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase1", testMethod: testMethod1);
			var testCase2 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase2", testMethod: testMethod2);

			var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
			var completionTask = Task.WhenAny(timeoutTask, Task.WhenAll(testClassTcs1.Task, testClassTcs2.Task));
			var runner = new TestableCoreTestCollectionRunner([testCase1, testCase2], runTestClass, parallelismOptions: ParallelismOptions.All);

			await runner.RunAsync();

			Assert.False(timeoutTask.IsCompleted, "Timed out waiting for test classes to run in parallel.");
			async ValueTask<RunSummary> runTestClass(ICoreTestClass testClass, IReadOnlyCollection<ICoreTestCase> testCases)
			{
				if (testClass == testClass1)
				{
					testClassTcs1.TrySetResult(true);
				}
				else
				{
					testClassTcs2.TrySetResult(true);
				}

				await completionTask;
				return new RunSummary { Total = 1 };
			}
		}

		[Fact]
		public static async ValueTask SerialTestClasses()
		{
			var messages = new List<string>();
			var testCollection = Mocks.CoreTestCollection();
			var testClass1 = Mocks.CoreTestClass(testClassName: "TestClass1", testCollection: testCollection);
			var testClass2 = Mocks.CoreTestClass(testClassName: "TestClass2", testCollection: testCollection);
			var testMethod1 = Mocks.CoreTestMethod(methodName: "TestMethod1", testClass: testClass1);
			var testMethod2 = Mocks.CoreTestMethod(methodName: "TestMethod2", testClass: testClass2);
			var testCase1 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase1", testMethod: testMethod1);
			var testCase2 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase2", testMethod: testMethod2);

			var runner = new TestableCoreTestCollectionRunner([testCase1, testCase2], runTestClass);

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

			async ValueTask<RunSummary> runTestClass(ICoreTestClass testClass, IReadOnlyCollection<ICoreTestCase> testCases)
			{
				messages.Add($"{testClass.TestClassName} pre-sleep");

				await Task.Delay(50, TestContext.Current.CancellationToken);

				messages.Add($"{testClass.TestClassName} post-sleep");

				return new RunSummary { Total = 1 };
			}
		}

		class MyThrowingOrderer : ITestClassOrderer
		{
			public IReadOnlyCollection<TTestClass?> OrderTestClasses<TTestClass>(IReadOnlyCollection<TTestClass?> testClasses)
				where TTestClass : notnull, ITestClass =>
					throw new DivideByZeroException();
		}
	}

	class TestableCoreTestCollectionRunner(ICoreTestCase[] testCases,
		Func<ICoreTestClass, IReadOnlyCollection<ICoreTestCase>, ValueTask<RunSummary>>? runTestClassLamda = null,
		ParallelismOptions parallelismOptions = ParallelismOptionsAliases.Default) :
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
				runTestClassLamda ?? ((_, _) => new(new RunSummary { Total = 1, })),
				parallelismOptions,
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
			Func<ICoreTestClass, IReadOnlyCollection<ICoreTestCase>, ValueTask<RunSummary>> runTestClassLamda,
			ParallelismOptions parallelismOptions,
			CancellationTokenSource cancellationTokenSource) :
			CoreTestCollectionRunnerContext<ICoreTestCollection, ICoreTestClass, ICoreTestCase>(
				testCollection,
				testCases,
				explicitOption,
				messageBus,
				aggregator,
				parallelismOptions,
				parallelizationSemaphore: null,
				cancellationTokenSource)
		{
			public override ValueTask<RunSummary> RunTestClass(
				ICoreTestClass testClass,
				IReadOnlyCollection<ICoreTestCase> testCases) => runTestClassLamda(testClass, testCases);
		}
	}
}
