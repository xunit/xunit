using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class XunitTestCaseRunnerTests
{
	public static class Messages
	{
		[Fact]
		public static async ValueTask Passing()
		{
			var test = TestData.XunitTest<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCaseRunner(test);

			await runner.Run();

			Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageBus.Messages,
				msg =>
				{
					var starting = Assert.IsType<ITestCaseStarting>(msg, exactMatch: false);
					verifyTestCaseMessage(starting);
					// Reading the assembly-level trait on the test project
					var trait = Assert.Single(starting.Traits);
					Assert.Equal("Assembly", trait.Key);
					var value = Assert.Single(trait.Value);
					Assert.Equal("Trait", value);
				},
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// Test method is invoked here
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => verifyTestCaseMessage(Assert.IsType<ITestCaseFinished>(msg, exactMatch: false))
			);

			static void verifyTestCaseMessage(ITestCaseMessage message)
			{
				Assert.Equal("assembly-id", message.AssemblyUniqueID);
				Assert.Equal("test-case-id", message.TestCaseUniqueID);
				Assert.Equal("test-class-id", message.TestClassUniqueID);
				Assert.Equal("test-collection-id", message.TestCollectionUniqueID);
				Assert.Equal("test-method-id", message.TestMethodUniqueID);
			}
		}

		[Fact]
		public static async ValueTask StaticPassing()
		{
			var test = TestData.XunitTest<ClassUnderTest>(nameof(ClassUnderTest.StaticPassing));
			var runner = new TestableXunitTestCaseRunner(test);

			await runner.Run();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				// Test method is invoked here
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask Failed()
		{
			var test = TestData.XunitTest<ClassUnderTest>(nameof(ClassUnderTest.Failing));
			var runner = new TestableXunitTestCaseRunner(test);

			await runner.Run();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// Test method is invoked here
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal(-1, failed.ExceptionParentIndices.Single());
					Assert.Equal("Xunit.Sdk.TrueException", failed.ExceptionTypes.Single());
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaAttribute()
		{
			var test = TestData.XunitTest<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaAttribute));
			var runner = new TestableXunitTestCaseRunner(test);

			await runner.Run();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("Don't run me", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaException()
		{
			var test = TestData.XunitTest<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaException));
			var runner = new TestableXunitTestCaseRunner(test);

			await runner.Run();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// Test method is invoked here
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("This isn't a good time", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaRegisteredException()
		{
			var test = TestData.XunitTest<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaRegisteredException));
			var runner = new TestableXunitTestCaseRunner(test);

			await runner.Run();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// Test method is invoked here
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("Dividing by zero is really tough", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask NotRun()
		{
			var test = TestData.XunitTest<ClassUnderTest>(nameof(ClassUnderTest.ExplicitTest));
			var runner = new TestableXunitTestCaseRunner(test);

			await runner.Run();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestNotRun>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false)
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
	}

	[Collection("Shared state in FixtureWithEvents")]
	public static class Fixtures
	{
		[Fact]
		public static async ValueTask FixtureEvents()
		{
			FixtureWithEvents.Events.Clear();

			var testCase = TestData.XunitTestCase<FixtureWithEventsClassUnderTest>(nameof(FixtureWithEventsClassUnderTest.TestMethod));
			var test = Assert.Single(await testCase.CreateTests());
			var runner = new TestableXunitTestCaseRunner(test);
			await runner.FixtureMappings.InitializeAsync(typeof(FixtureWithEvents));

			await runner.Run();

			Assert.Collection(
				FixtureWithEvents.Events,
				e => Assert.Equal("OnTestCaseStarting", e),
				e => Assert.Equal("OnTestCaseStartingAsync", e),

				e => Assert.Equal("OnTestStarting", e),
				e => Assert.Equal("OnTestStartingAsync", e),
				e => Assert.Equal("OnTestFinishedAsync", e),
				e => Assert.Equal("OnTestFinished", e),

				e => Assert.Equal("OnTestCaseFinishedAsync", e),
				e => Assert.Equal("OnTestCaseFinished", e)
			);
		}

		class FixtureWithEventsClassUnderTest
		{
			[Fact]
			public void TestMethod() { }
		}
	}

	class TestableXunitTestCaseRunner(IXunitTest test) :
		XunitTestCaseRunner
	{
		public ExceptionAggregator Aggregator = new();
		public CancellationTokenSource CancellationTokenSource = new();
		public FixtureMappingManager FixtureMappings = new("Mock");
		public SpyMessageBus MessageBus = new();

		public async ValueTask<RunSummary> Run()
		{
			await using var ctxt = new XunitTestCaseRunnerContext(
				test.TestCase,
				[test],
				MessageBus,
				Aggregator,
				CancellationTokenSource,
				test.TestCase.TestCaseDisplayName,
				test.TestCase.SkipReason,
				ExplicitOption.Off,
				[],
				FixtureMappings
			);
			await ctxt.InitializeAsync();

			return await Run(ctxt);
		}
	}
}
