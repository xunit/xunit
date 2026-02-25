using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class CoreTestAssemblyRunnerTests
{
	[CollectionDefinition(DisableParallelization = true)]
	[Collection(typeof(Run))]
	public class Run
	{
		protected readonly SpyMessageSink DiagnosticMessageSink = SpyMessageSink.Capture();

		public Run() =>
			TestContextInternal.Current.DiagnosticMessageSink = DiagnosticMessageSink;

		[Fact]
		public async ValueTask OrdersTestCollections()
		{
			var testAssembly = Mocks.CoreTestAssembly(testCollectionOrderer: UnorderedTestCollectionOrderer.Instance);
			var testCollection1 = Mocks.CoreTestCollection(testAssembly: testAssembly, testCollectionDisplayName: "test-collection-1", uniqueID: "1");
			var testCase1 = testCaseForCollection(testCollection1, "test-case-1");
			var testCollection2 = Mocks.CoreTestCollection(testAssembly: testAssembly, testCollectionDisplayName: "test-collection-2", uniqueID: "2");
			var testCase2 = testCaseForCollection(testCollection2, "test-case-2");
			var testCollection3 = Mocks.CoreTestCollection(testAssembly: testAssembly, testCollectionDisplayName: "test-collection-3", uniqueID: "3");
			var testCase3 = testCaseForCollection(testCollection3, "test-case-3");
			var options = TestData.TestFrameworkExecutionOptions(disableParallelization: true);
			var runner = new TestableCoreTestAssemblyRunner([testCase3, testCase1, testCase2], options);

			await runner.RunAsync();

			Assert.Collection(
				runner.TestCollectionsRun,
				tc =>
				{
					Assert.Equal("test-collection-3", tc.TestCollection.TestCollectionDisplayName);
					Assert.Equal(["test-case-3"], tc.TestCases.Select(tc => tc.TestCaseDisplayName));
				},
				tc =>
				{
					Assert.Equal("test-collection-1", tc.TestCollection.TestCollectionDisplayName);
					Assert.Equal(["test-case-1"], tc.TestCases.Select(tc => tc.TestCaseDisplayName));
				},
				tc =>
				{
					Assert.Equal("test-collection-2", tc.TestCollection.TestCollectionDisplayName);
					Assert.Equal(["test-case-2"], tc.TestCases.Select(tc => tc.TestCaseDisplayName));
				}
			);

			static ICoreTestCase testCaseForCollection(
				ICoreTestCollection testCollection,
				string testCaseDisplayName) =>
					Mocks.CoreTestCase(testMethod: Mocks.CoreTestMethod(testClass: Mocks.CoreTestClass(testCollection: testCollection)), testCaseDisplayName: testCaseDisplayName);
		}

		[Fact]
		public async ValueTask ThrowingOrderer()
		{
			var testAssembly = Mocks.CoreTestAssembly(testCollectionOrderer: new MyThrowingOrderer());
			var testCollection = Mocks.CoreTestCollection(testAssembly: testAssembly);
			var testClass = Mocks.CoreTestClass(testCollection: testCollection);
			var testMethod = Mocks.CoreTestMethod(testClass: testClass);
			var testCase = Mocks.CoreTestCase(testMethod: testMethod);
			var runner = new TestableCoreTestAssemblyRunner([testCase]);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg =>
				{
					var error = Assert.IsType<IErrorMessage>(msg, exactMatch: false);
					Assert.Equal(typeof(TestPipelineException).SafeName(), error.ExceptionTypes.Single());
					Assert.Equal($"Test collection orderer '{typeof(MyThrowingOrderer).SafeName()}' threw '{typeof(DivideByZeroException).SafeName()}' during ordering: Attempted to divide by zero.", error.Messages.Single());
				},
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		class MyThrowingOrderer : ITestCollectionOrderer
		{
			public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections)
				where TTestCollection : ITestCollection =>
					throw new DivideByZeroException();
		}

		[Theory]
		[InlineData(ParallelAlgorithm.Aggressive, typeof(MaxConcurrencySyncContext))]
		[InlineData(ParallelAlgorithm.Conservative, null)]
		public async ValueTask AlgorithmImpactsSyncContext(
			ParallelAlgorithm parallelAlgorithm,
			Type? expectedSyncContextType)
		{
			// Need to use Task.Run to get ourselves a "clean" execution context
			await Task.Run(async () =>
			{
				var testCase = Mocks.CoreTestCase();
				var options = TestData.TestFrameworkExecutionOptions(parallelAlgorithm: parallelAlgorithm);
				var runner = new TestableCoreTestAssemblyRunner([testCase], options);

				await runner.RunAsync();

				Assert.Equal(expectedSyncContextType, runner.RunTestCollection_SyncContext?.GetType());
			}, TestContext.Current.CancellationToken);
		}

		[Fact]
		public async ValueTask Parallel_Conversative()
		{
			var testCollection1 = Mocks.CoreTestCollection(uniqueID: "1");
			var testCase1 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase1", testMethod: Mocks.CoreTestMethod(testClass: Mocks.CoreTestClass(testCollection: testCollection1)));
			var testCollection2 = Mocks.CoreTestCollection(uniqueID: "2");
			var testCase2 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase2", testMethod: Mocks.CoreTestMethod(testClass: Mocks.CoreTestClass(testCollection: testCollection2)));
			var options = TestData.TestFrameworkExecutionOptions(maxParallelThreads: 1, parallelAlgorithm: ParallelAlgorithm.Conservative);
			var runner = new TestableCoreTestAssemblyRunner([testCase1, testCase2], options);

			await runner.RunAsync();

			// Conservative will let each test finish before the next one runs, despite sleeping. However, we don't know which one
			// gets to go first, so we look at the first one to see which one it is, and make sure the post-sleep happens
			// directly after the pre-sleep
			var messages = DiagnosticMessageSink.Messages.OfType<IDiagnosticMessage>().Select(m => m.Message).ToArray();
			var firstMessage = messages[0];
			Assert.Contains("pre-sleep", firstMessage);
			Assert.Equal(firstMessage.Replace("pre-sleep", "post-sleep"), messages[1]);

			var thirdMessage = messages[2];
			Assert.NotEqual(firstMessage, thirdMessage);
			Assert.Contains("pre-sleep", thirdMessage);
			Assert.Equal(thirdMessage.Replace("pre-sleep", "post-sleep"), messages[3]);
		}

		// TODO: The 50ms delay here is to try to deal with this test being flaky, since it's counting on the idea that
		// you can guarantee the 2nd test gets scheduled to run while the first test is in an await. When running this
		// test assembly alone, that's generally not a problem; when running a bunch of test assemblies in parallel, the
		// indeterminism goes up dramatically. This test might need to just go away if it turns out to be too flaky.
		[Fact]
		public async ValueTask Parallel_Aggressive()
		{
			var testCollection1 = Mocks.CoreTestCollection(uniqueID: "1");
			var testCase1 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase1", testMethod: Mocks.CoreTestMethod(testClass: Mocks.CoreTestClass(testCollection: testCollection1)));
			var testCollection2 = Mocks.CoreTestCollection(uniqueID: "2");
			var testCase2 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase2", testMethod: Mocks.CoreTestMethod(testClass: Mocks.CoreTestClass(testCollection: testCollection2)));
			var options = TestData.TestFrameworkExecutionOptions(maxParallelThreads: 1, parallelAlgorithm: ParallelAlgorithm.Aggressive);
			var runner = new TestableCoreTestAssemblyRunner([testCase1, testCase2], options, testDelay: 50);

			await runner.RunAsync();

			// Agressive will let each the second test start while the first test sleeps, so we should see two pre-sleep
			// messages and then two post-sleep messages. We cannot know anything else about the order, though, because
			// the "first" one that sleeps is not necessarily the "first" one that wakes.
			Assert.Collection(
				DiagnosticMessageSink.Messages.OfType<IDiagnosticMessage>().Select(m => m.Message),
				msg => Assert.Contains("pre-sleep", msg),
				msg => Assert.Contains("pre-sleep", msg),
				msg => Assert.Contains("post-sleep", msg),
				msg => Assert.Contains("post-sleep", msg)
			);
		}

		[Fact]
		public async ValueTask NonParallel()
		{
			var testCollection1 = Mocks.CoreTestCollection(uniqueID: "1");
			var testCase1 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase1", testMethod: Mocks.CoreTestMethod(testClass: Mocks.CoreTestClass(testCollection: testCollection1)));
			var testCollection2 = Mocks.CoreTestCollection(uniqueID: "2");
			var testCase2 = Mocks.CoreTestCase(testCaseDisplayName: "TestCase2", testMethod: Mocks.CoreTestMethod(testClass: Mocks.CoreTestClass(testCollection: testCollection2)));
			var options = TestData.TestFrameworkExecutionOptions(disableParallelization: true);
			var runner = new TestableCoreTestAssemblyRunner([testCase1, testCase2], options);

			await runner.RunAsync();

			// When it's non-parallel, we should always get pre, post, pre, post, though we don't
			// necessarily know which one comes first.
			var messages = DiagnosticMessageSink.Messages.OfType<IDiagnosticMessage>().Select(m => m.Message).ToArray();
			Assert.Equal(4, messages.Length);
			var firstPreSleep = messages[0];
			Assert.EndsWith("pre-sleep", firstPreSleep);
			Assert.Equal(firstPreSleep.Replace("pre-", "post-"), messages[1]);
			var secondPreSleep = messages[2];
			Assert.EndsWith("pre-sleep", secondPreSleep);
			Assert.Equal(secondPreSleep.Replace("pre-", "post-"), messages[3]);
		}
	}

	class TestableCoreTestAssemblyRunner(
		ICoreTestCase[] testCases,
		ITestFrameworkExecutionOptions? executionOptions = null,
		int testDelay = 0) :
			CoreTestAssemblyRunner<TestableCoreTestAssemblyRunner.TestableContext, ICoreTestAssembly, ICoreTestCollection, ICoreTestCase>
	{
		public readonly SpyMessageSink MessageSink = SpyMessageSink.Capture();
		public List<(ICoreTestCollection TestCollection, IReadOnlyCollection<ICoreTestCase> TestCases, Exception? Exception)> TestCollectionsRun = [];

		protected override ValueTask<RunSummary> FailTestCollection(
			TestableContext ctxt,
			ICoreTestCollection testCollection,
			IReadOnlyCollection<ICoreTestCase> testCases,
			Exception exception)
		{
			TestCollectionsRun.Add((testCollection, testCases, exception));

			return base.FailTestCollection(ctxt, testCollection, testCases, exception);
		}

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var context = new TestableContext(
				testCases[0].TestCollection.TestAssembly,
				testCases,
				MessageSink,
				executionOptions ?? TestData.TestFrameworkExecutionOptions(),
				CancellationToken.None,
				testDelay
			);
			await context.InitializeAsync();

			return await Run(context);
		}

		public SynchronizationContext? RunTestCollection_SyncContext;

		protected override ValueTask<RunSummary> RunTestCollection(
			TestableContext ctxt,
			ICoreTestCollection testCollection,
			IReadOnlyCollection<ICoreTestCase> testCases)
		{
			RunTestCollection_SyncContext = SynchronizationContext.Current;

			TestCollectionsRun.Add((testCollection, testCases, null));

			return base.RunTestCollection(ctxt, testCollection, testCases);
		}

		protected override ValueTask<string> GetTestFrameworkDisplayName(TestableContext ctxt) =>
			new("<TestFramework>");

		public class TestableContext(
			ICoreTestAssembly testAssembly,
			IReadOnlyCollection<ICoreTestCase> testCases,
			IMessageSink executionMessageSink,
			ITestFrameworkExecutionOptions executionOptions,
			CancellationToken cancellationToken,
			int testDelay) :
				CoreTestAssemblyRunnerContext<ICoreTestAssembly, ICoreTestCollection, ICoreTestCase>(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken)
		{
			public override async ValueTask<RunSummary> RunTestCollection(
				ICoreTestCollection testCollection,
				IReadOnlyCollection<ICoreTestCase> testCases)
			{
				await BeforeTestCollection();

				try
				{
					foreach (var testCase in testCases)
					{
						TestContext.Current.SendDiagnosticMessage($"{testCase.TestCaseDisplayName} pre-sleep");

						if (testDelay == 0)
							await Task.Yield();
						else
							await Task.Delay(testDelay);

						TestContext.Current.SendDiagnosticMessage($"{testCase.TestCaseDisplayName} post-sleep");
					}

					return new RunSummary();
				}
				finally
				{
					AfterTestCollection();
				}
			}

			protected override string GetTestCollectionFactoryDisplayName() =>
				"<DefaultTestCollectionFactory>";
		}
	}
}
