using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestAssemblyRunnerTests
{
	public class Messages
	{
		[Fact]
		public static async ValueTask Passing()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			//Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageSink.Messages,
				msg =>
				{
					var starting = Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false);
					verifyTestAssemblyMessage(starting);
#if BUILD_X86 && NETFRAMEWORK
					Assert.Equal("xunit.v3.core.netfx.x86.tests", starting.AssemblyName);
#elif BUILD_X86
					Assert.Equal("xunit.v3.core.netcore.x86.tests", starting.AssemblyName);
#elif NETFRAMEWORK
					Assert.Equal("xunit.v3.core.netfx.tests", starting.AssemblyName);
#else
					Assert.Equal("xunit.v3.core.netcore.tests", starting.AssemblyName);
#endif
					Assert.Equal(typeof(ClassUnderTest).Assembly.Location, starting.AssemblyPath);
					Assert.Null(starting.ConfigFilePath);
					Assert.NotNull(starting.Seed);  // We don't know what the seed will be, we just know it will have one
#if NET472
					Assert.Equal(".NETFramework,Version=v4.7.2", starting.TargetFramework);
#elif NET8_0
					Assert.Equal(".NETCoreApp,Version=v8.0", starting.TargetFramework);
#else
#error Unknown target framework
#endif
					Assert.Matches($"^{IntPtr.Size * 8}-bit \\({Regex.Escape(RuntimeInformation.ProcessArchitecture.ToDisplayName())}\\) {Regex.Escape(RuntimeInformation.FrameworkDescription)} \\[collection-per-class, parallel \\(\\d+ threads\\)\\]$", starting.TestEnvironment);
					Assert.Matches("^xUnit.net v3 \\d+.\\d+.\\d+", starting.TestFrameworkDisplayName);
					// Trait comes from an assembly-level trait attribute on this test assembly
					var trait = Assert.Single(starting.Traits);
					Assert.Equal("Assembly", trait.Key);
					var value = Assert.Single(trait.Value);
					Assert.Equal("Trait", value);
				},
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => verifyTestAssemblyMessage(Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false))
			);

			static void verifyTestAssemblyMessage(ITestAssemblyMessage message) =>
				Assert.Equal("assembly-id", message.AssemblyUniqueID);
		}

		[Fact]
		public static async ValueTask StaticPassing()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.StaticPassing));
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask Failed()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Failing));
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal(-1, failed.ExceptionParentIndices.Single());
					Assert.Equal("Xunit.Sdk.TrueException", failed.ExceptionTypes.Single());
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaAttribute()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaAttribute));
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				// ...no invocation since it's skipped...
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("Don't run me", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaException()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaException));
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("This isn't a good time", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaRegisteredException()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaRegisteredException));
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("Dividing by zero is really tough", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask NotRun()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.ExplicitTest));
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestNotRun>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		class ClassUnderTest : IDisposable
		{
			public void Dispose() { }

			[Fact]
			public async Task Passing() => await Task.Yield();

			[Fact]
			public static void StaticPassing() { }

			[Fact]
			public void Failing() => Assert.True(false);

			[Fact(Skip = "Don't run me")]
			public void SkippedViaAttribute() { }

			[Fact]
			public void SkippedViaException() => Assert.Skip("This isn't a good time");

			[Fact(SkipExceptions = [typeof(DivideByZeroException)])]
			public void SkippedViaRegisteredException() => throw new DivideByZeroException("Dividing by zero is really tough");

			[Fact(Explicit = true)]
			public void ExplicitTest() => Assert.Fail("Should not run");
		}

		[Fact]
		public static async ValueTask AssemblyFixtureThrowingDuringInitialization()
		{
			var testAssembly = Mocks.XunitTestAssembly(assemblyFixtureTypes: [typeof(ThrowingInitFixture)]);
			var testCollection = TestData.XunitTestCollection(testAssembly);
			var testClass = TestData.XunitTestClass(typeof(ClassUnderTest), testCollection);
			var methodInfo = Guard.NotNull("Could not find method", typeof(ClassUnderTest).GetMethod(nameof(ClassUnderTest.Passing)));
			var testMethod = TestData.XunitTestMethod(testClass, methodInfo);
			var testCase = TestData.XunitTestCase(testMethod);
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal([-1, 0], failed.ExceptionParentIndices);
					Assert.Equal(new string[] { typeof(TestPipelineException).SafeName(), typeof(DivideByZeroException).SafeName() }, failed.ExceptionTypes);
					Assert.Equal(new string[] { $"Assembly fixture type '{typeof(ThrowingInitFixture).SafeName()}' threw in its constructor", "Attempted to divide by zero." }, failed.Messages);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		class ThrowingInitFixture
		{
			public ThrowingInitFixture() => throw new DivideByZeroException();
		}

		[Fact]
		public static async ValueTask AssemblyFixtureThrowingDuringCleanup()
		{
			var testAssembly = Mocks.XunitTestAssembly(assemblyFixtureTypes: [typeof(ThrowingDisposeFixture)]);
			var testCollection = TestData.XunitTestCollection(testAssembly);
			var testClass = TestData.XunitTestClass(typeof(ClassUnderTest), testCollection);
			var methodInfo = Guard.NotNull("Could not find method", typeof(ClassUnderTest).GetMethod(nameof(ClassUnderTest.Passing)));
			var testMethod = TestData.XunitTestMethod(testClass, methodInfo);
			var testCase = TestData.XunitTestCase(testMethod);
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg =>
				{
					var failure = Assert.IsType<ITestAssemblyCleanupFailure>(msg, exactMatch: false);
					Assert.Equal([-1, 0], failure.ExceptionParentIndices);
					Assert.Equal(new string[] { typeof(TestPipelineException).SafeName(), typeof(DivideByZeroException).SafeName() }, failure.ExceptionTypes);
					Assert.Equal(new string[] { $"Assembly fixture type '{typeof(ThrowingDisposeFixture).SafeName()}' threw in Dispose", "Attempted to divide by zero." }, failure.Messages);
				},
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		class ThrowingDisposeFixture : IDisposable
		{
			public void Dispose() => throw new DivideByZeroException();
		}
	}

	public class OrderTestCollections
	{
		[Fact]
		public static async ValueTask UsesCustomOrderers()
		{
			var testAssembly = Mocks.XunitTestAssembly(testCaseOrderer: new CustomTestCaseOrderer(), testCollectionOrderer: new CustomTestCollectionOrderer());
			var testCollection1 = Mocks.XunitTestCollection(testAssembly: testAssembly, testCollectionDisplayName: "1", uniqueID: "1");
			var testCase1a = testCaseForCollection(testCollection1, "1a");
			var testCase1b = testCaseForCollection(testCollection1, "1b");
			var testCollection2 = Mocks.XunitTestCollection(testAssembly: testAssembly, testCollectionDisplayName: "2", uniqueID: "2");
			var testCase2a = testCaseForCollection(testCollection2, "2a");
			var testCase2b = testCaseForCollection(testCollection2, "2b");
			var options = TestData.TestFrameworkExecutionOptions(disableParallelization: true);
			var runner = new TestableXunitTestAssemblyRunner(testCase1a, testCase2a, testCase2b, testCase1b) { ExecutionOptions = options };

			await runner.RunAsync();

			Assert.IsType<CustomTestCollectionOrderer>(runner.RunTestCollectionsAsync_TestCollectionOrderer);
			Assert.Collection(
				runner.TestCollectionsRun,
				first =>
				{
					Assert.Equal("2", first.TestCollection.TestCollectionDisplayName);
					Assert.Equal(["2b", "2a"], first.TestCases.Select(tc => tc.TestCaseDisplayName));
				},
				second =>
				{
					Assert.Equal("1", second.TestCollection.TestCollectionDisplayName);
					Assert.Equal(["1b", "1a"], second.TestCases.Select(tc => tc.TestCaseDisplayName));
				}
			);

			static IXunitTestCase testCaseForCollection(
				IXunitTestCollection testCollection,
				string testCaseDisplayName) =>
					Mocks.XunitTestCase(testMethod: Mocks.XunitTestMethod(testClass: Mocks.XunitTestClass(testCollection: testCollection)), testCaseDisplayName: testCaseDisplayName);
		}

		class CustomTestCaseOrderer : ITestCaseOrderer
		{
			public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
				where TTestCase : notnull, ITestCase =>
					testCases.OrderByDescending(tc => tc.TestCaseDisplayName).CastOrToReadOnlyCollection();
		}

		class CustomTestCollectionOrderer : ITestCollectionOrderer
		{
			public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections)
				where TTestCollection : ITestCollection =>
					testCollections.OrderByDescending(tc => tc.TestCollectionDisplayName).CastOrToReadOnlyCollection();
		}
	}

	public class RunTestCollectionsAsync
	{
		[Theory]
		[InlineData(ParallelAlgorithm.Aggressive, typeof(MaxConcurrencySyncContext))]
		[InlineData(ParallelAlgorithm.Conservative, null)]
		public static async ValueTask AlgorithmImpactsSyncContext(
			ParallelAlgorithm parallelAlgorithm,
			Type? expectedSyncContextType)
		{
			// Need to use Task.Run to get ourselves a "clean" execution context
			await Task.Run(async () =>
			{
				var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
				var options = TestData.TestFrameworkExecutionOptions(parallelAlgorithm: parallelAlgorithm);
				var runner = new TestableXunitTestAssemblyRunner(testCase) { ExecutionOptions = options };

				await runner.RunAsync();

				Assert.Equal(expectedSyncContextType, runner.RunTestCollectionAsync_SyncContext?.GetType());
			}, TestContext.Current.CancellationToken);
		}

		[Fact]
		public static async ValueTask Parallel_Conversative()
		{
			var spyMessageSink = SpyMessageSink.Capture();
			TestContextInternal.Current.DiagnosticMessageSink = spyMessageSink;
			var testCollection1 = Mocks.XunitTestCollection(uniqueID: "1");
			var test1 = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.ParallelTest1), testCollection: testCollection1);
			var testCollection2 = Mocks.XunitTestCollection(uniqueID: "2");
			var test2 = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.ParallelTest2), testCollection: testCollection2);
			var options = TestData.TestFrameworkExecutionOptions(maxParallelThreads: 1, parallelAlgorithm: ParallelAlgorithm.Conservative);
			var runner = new TestableXunitTestAssemblyRunner(test1, test2) { ExecutionOptions = options };

			await runner.RunAsync();

			// Conservative will let each test finish before the next one runs, despite sleeping. However, we don't know which one
			// gets to go first, so we look at the first one to see which one it is, and make sure the post-sleep happens
			// directly after the pre-sleep
			var messages = spyMessageSink.Messages.OfType<IDiagnosticMessage>().Select(m => m.Message).ToArray();
			var firstMessage = messages[0];
			Assert.Contains("pre-sleep", firstMessage);
			Assert.Equal(firstMessage.Replace("pre-sleep", "post-sleep"), messages[1]);

			var thirdMessage = messages[2];
			Assert.NotEqual(firstMessage, thirdMessage);
			Assert.Contains("pre-sleep", thirdMessage);
			Assert.Equal(thirdMessage.Replace("pre-sleep", "post-sleep"), messages[3]);
		}

		[Fact]
		public static async ValueTask Parallel_Aggressive()
		{
			var spyMessageSink = SpyMessageSink.Capture();
			TestContextInternal.Current.DiagnosticMessageSink = spyMessageSink;
			var testCollection1 = Mocks.XunitTestCollection(uniqueID: "1");
			var test1 = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.ParallelTest1), testCollection: testCollection1);
			var testCollection2 = Mocks.XunitTestCollection(uniqueID: "2");
			var test2 = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.ParallelTest2), testCollection: testCollection2);
			var options = TestData.TestFrameworkExecutionOptions(maxParallelThreads: 1, parallelAlgorithm: ParallelAlgorithm.Aggressive);
			var runner = new TestableXunitTestAssemblyRunner(test1, test2) { ExecutionOptions = options };

			await runner.RunAsync();

			// Agressive will let each the second test start while the first test sleeps, so we should see two pre-sleep
			// messages and then two post-sleep messages. We cannot know anything else about the order, though, because
			// the "first" one that sleeps is not necessarily the "first" one that wakes.
			Assert.Collection(
				spyMessageSink.Messages.OfType<IDiagnosticMessage>().Select(m => m.Message),
				msg => Assert.Contains("pre-sleep", msg),
				msg => Assert.Contains("pre-sleep", msg),
				msg => Assert.Contains("post-sleep", msg),
				msg => Assert.Contains("post-sleep", msg)
			);
		}

		[Fact]
		public static async ValueTask NonParallel()
		{
			var spyMessageSink = SpyMessageSink.Capture();
			TestContextInternal.Current.DiagnosticMessageSink = spyMessageSink;
			var testCollection1 = Mocks.XunitTestCollection(uniqueID: "1");
			var test1 = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.ParallelTest1), testCollection: testCollection1);
			var testCollection2 = Mocks.XunitTestCollection(uniqueID: "2");
			var test2 = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.ParallelTest2), testCollection: testCollection2);
			var options = TestData.TestFrameworkExecutionOptions(disableParallelization: true);
			var runner = new TestableXunitTestAssemblyRunner(test1, test2) { ExecutionOptions = options };

			await runner.RunAsync();

			// When it's non-parallel, we should always get pre, post, pre, post, though we don't
			// necessarily know which one comes first.
			var messages = spyMessageSink.Messages.OfType<IDiagnosticMessage>().Select(m => m.Message).ToArray();
			Assert.Equal(4, messages.Length);
			var firstPreSleep = messages[0];
			Assert.EndsWith("pre-sleep", firstPreSleep);
			Assert.Equal(firstPreSleep.Replace("pre-", "post-"), messages[1]);
			var secondPreSleep = messages[2];
			Assert.EndsWith("pre-sleep", secondPreSleep);
			Assert.Equal(secondPreSleep.Replace("pre-", "post-"), messages[3]);
		}
	}

	class ClassUnderTest
	{
		[Fact]
		public void Passing() { }

		[Fact]
		public async ValueTask ParallelTest1()
		{
			TestContext.Current.SendDiagnosticMessage("ParallelTest1 pre-sleep");
			await Task.Delay(50, TestContext.Current.CancellationToken);
			TestContext.Current.SendDiagnosticMessage("ParallelTest1 post-sleep");
		}

		[Fact]
		public async ValueTask ParallelTest2()
		{
			TestContext.Current.SendDiagnosticMessage("ParallelTest2 pre-sleep");
			await Task.Delay(50, TestContext.Current.CancellationToken);
			TestContext.Current.SendDiagnosticMessage("ParallelTest2 post-sleep");
		}
	}

	class TestableXunitTestAssemblyRunner(params IXunitTestCase[] testCases) :
		XunitTestAssemblyRunner
	{
		public ITestFrameworkExecutionOptions ExecutionOptions = TestData.TestFrameworkExecutionOptions();
		public readonly SpyMessageSink MessageSink = SpyMessageSink.Capture();
		public readonly IReadOnlyCollection<IXunitTestCase> TestCases = testCases.Length == 0 ? [TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.ParallelTest1))] : testCases;
		public List<(IXunitTestCollection TestCollection, IReadOnlyCollection<IXunitTestCase> TestCases, Exception? Exception)> TestCollectionsRun = [];

		protected override ValueTask<RunSummary> FailTestCollection(
			XunitTestAssemblyRunnerContext ctxt,
			IXunitTestCollection testCollection,
			IReadOnlyCollection<IXunitTestCase> testCases,
			Exception exception)
		{
			// The usage of the test case orderer is in another component, so we'll just order the test cases
			// here before putting them into the list, so that we can show the proposed impact of the orderer
			var testCaseOrderer = ctxt.AssemblyTestCaseOrderer ?? DefaultTestCaseOrderer.Instance;
			TestCollectionsRun.Add((testCollection, testCaseOrderer.OrderTestCases(testCases), exception));

			return base.FailTestCollection(ctxt, testCollection, testCases, exception);
		}

		public ValueTask<RunSummary> RunAsync() =>
			Run(TestCases.First().TestCollection.TestAssembly, TestCases, MessageSink, ExecutionOptions, default);

		public SynchronizationContext? RunTestCollectionAsync_SyncContext;

		protected override ValueTask<RunSummary> RunTestCollection(
			XunitTestAssemblyRunnerContext ctxt,
			IXunitTestCollection testCollection,
			IReadOnlyCollection<IXunitTestCase> testCases)
		{
			RunTestCollectionAsync_SyncContext = SynchronizationContext.Current;

			// The usage of the test case orderer is in another component, so we'll just order the test cases
			// here before putting them into the list, so that we can show the proposed impact of the orderer
			var testCaseOrderer = ctxt.AssemblyTestCaseOrderer ?? DefaultTestCaseOrderer.Instance;
			TestCollectionsRun.Add((testCollection, testCaseOrderer.OrderTestCases(testCases), null));

			return base.RunTestCollection(ctxt, testCollection, testCases);
		}

		public ITestCollectionOrderer? RunTestCollectionsAsync_TestCollectionOrderer;

		protected override ValueTask<RunSummary> RunTestCollections(
			XunitTestAssemblyRunnerContext ctxt,
			Exception? exception)
		{
			RunTestCollectionsAsync_TestCollectionOrderer = ctxt.TestAssembly.TestCollectionOrderer;

			return base.RunTestCollections(ctxt, exception);
		}
	}
}
