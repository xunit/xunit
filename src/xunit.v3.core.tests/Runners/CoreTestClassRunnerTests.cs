using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class CoreTestClassRunnerTests
{
	public static class Run
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
			var runner = new TestableCoreTestClassRunner([testCase3, testCase1, testCase2]);

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
		public static async ValueTask ThrowingOrderer()
		{
			var testClass = Mocks.CoreTestClass(testMethodOrderer: new MyThrowingOrderer());
			var testMethod = Mocks.CoreTestMethod(testClass: testClass);
			var testCase = Mocks.CoreTestCase(testMethod: testMethod);
			var runner = new TestableCoreTestClassRunner([testCase]);

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

		[Fact]
		public static async ValueTask ParallelTestMethods()
		{
			var testMethodTcs1 = new TaskCompletionSource<bool>(TaskCreationOptions.None);
			var testMethodTcs2 = new TaskCompletionSource<bool>(TaskCreationOptions.None);

			var testClass = Mocks.CoreTestClass();
			var testMethod1 = Mocks.CoreTestMethod(methodName: "TestMethod1", testClass: testClass);
			var testMethod2 = Mocks.CoreTestMethod(methodName: "TestMethod2", testClass: testClass);
			var testCase1 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase1", testMethod: testMethod1);
			var testCase2 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase2", testMethod: testMethod2);

			var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
			var completionTask = Task.WhenAny(timeoutTask, Task.WhenAll(testMethodTcs1.Task, testMethodTcs2.Task));
			var runner = new TestableCoreTestClassRunner([testCase1, testCase2], runTestMethod, parallelismOptions: ParallelismOptions.All);

			await runner.RunAsync();

			Assert.False(timeoutTask.IsCompleted, "Timed out waiting for test methods to run in parallel.");
			async ValueTask<RunSummary> runTestMethod(ICoreTestMethod testMethod, IReadOnlyCollection<ICoreTestCase> testCases)
			{
				if (testMethod == testMethod1)
				{
					testMethodTcs1.TrySetResult(true);
				}
				else
				{
					testMethodTcs2.TrySetResult(true);
				}

				await completionTask;
				return new RunSummary { Total = 1 };
			}
		}

		[Fact]
		public static async ValueTask SerialTestMethods()
		{
			var messages = new List<string>();
			var testClass = Mocks.CoreTestClass();
			var testMethod1 = Mocks.CoreTestMethod(methodName: "TestMethod1", testClass: testClass);
			var testMethod2 = Mocks.CoreTestMethod(methodName: "TestMethod2", testClass: testClass);
			var testCase1 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase1", testMethod: testMethod1);
			var testCase2 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase2", testMethod: testMethod2);

			var runner = new TestableCoreTestClassRunner([testCase1, testCase2], runTestMethod);

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

			async ValueTask<RunSummary> runTestMethod(ICoreTestMethod testMethod, IReadOnlyCollection<ICoreTestCase> testCases)
			{
				messages.Add($"{testMethod.MethodName} pre-sleep");

				await Task.Delay(50, TestContext.Current.CancellationToken);

				messages.Add($"{testMethod.MethodName} post-sleep");

				return new RunSummary { Total = 1 };
			}
		}

		class MyThrowingOrderer : ITestMethodOrderer
		{
			public IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(IReadOnlyCollection<TTestMethod?> testMethods)
				where TTestMethod : notnull, ITestMethod =>
					throw new DivideByZeroException();
		}
	}

	class TestableCoreTestClassRunner(
		ICoreTestCase[] testCases,
		Func<ICoreTestMethod, IReadOnlyCollection<ICoreTestCase>, ValueTask<RunSummary>>? runTestMethodLamda = null,
		ParallelismOptions parallelismOptions = ParallelismOptionsAliases.Default) :
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
				parallelismOptions,
				runTestMethodLamda ?? ((_, _) => new ValueTask<RunSummary>(new RunSummary { Total = 1 })),
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
			ParallelismOptions parallelismOptions,
			Func<ICoreTestMethod, IReadOnlyCollection<ICoreTestCase>, ValueTask<RunSummary>> runTestMethodLamda,
			CancellationTokenSource cancellationTokenSource) :
			CoreTestClassRunnerContext<ICoreTestClass, ICoreTestMethod, ICoreTestCase>(testClass, testCases,
				explicitOption, messageBus, aggregator, parallelismOptions, cancellationTokenSource)
		{
			public override ValueTask<RunSummary> RunTestMethod(
				ICoreTestMethod testMethod,
				IReadOnlyCollection<ICoreTestCase> testCases) => runTestMethodLamda(testMethod, testCases);
		}
	}
}
