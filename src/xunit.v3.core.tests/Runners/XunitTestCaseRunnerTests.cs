using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestCaseRunnerTests
{
	public class Messages
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
					var starting = Assert.IsAssignableFrom<ITestCaseStarting>(msg);
					verifyTestCaseMessage(starting);
					// Reading the assembly-level trait on the test project
					var trait = Assert.Single(starting.Traits);
					Assert.Equal("Assembly", trait.Key);
					var value = Assert.Single(trait.Value);
					Assert.Equal("Trait", value);
				},
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg),
				// Test method is invoked here
				msg => Assert.IsAssignableFrom<ITestClassDisposeStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassDisposeFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestPassed>(msg),
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => verifyTestCaseMessage(Assert.IsAssignableFrom<ITestCaseFinished>(msg))
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
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				// Test method is invoked here
				msg => Assert.IsAssignableFrom<ITestPassed>(msg),
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg)
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
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg),
				// Test method is invoked here
				msg => Assert.IsAssignableFrom<ITestClassDisposeStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassDisposeFinished>(msg),
				msg =>
				{
					var failed = Assert.IsAssignableFrom<ITestFailed>(msg);
					Assert.Equal(-1, failed.ExceptionParentIndices.Single());
					Assert.Equal("Xunit.Sdk.TrueException", failed.ExceptionTypes.Single());
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg)
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
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg =>
				{
					var skipped = Assert.IsAssignableFrom<ITestSkipped>(msg);
					Assert.Equal("Don't run me", skipped.Reason);
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg)
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
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg),
				// Test method is invoked here
				msg => Assert.IsAssignableFrom<ITestClassDisposeStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassDisposeFinished>(msg),
				msg =>
				{
					var skipped = Assert.IsAssignableFrom<ITestSkipped>(msg);
					Assert.Equal("This isn't a good time", skipped.Reason);
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg)
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
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg),
				// Test method is invoked here
				msg => Assert.IsAssignableFrom<ITestClassDisposeStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassDisposeFinished>(msg),
				msg =>
				{
					var skipped = Assert.IsAssignableFrom<ITestSkipped>(msg);
					Assert.Equal("Dividing by zero is really tough", skipped.Reason);
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg)
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
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestNotRun>(msg),
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg)
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

	class TestableXunitTestCaseRunner(IXunitTest test) :
		XunitTestCaseRunner
	{
		public ExceptionAggregator Aggregator = new();
		public CancellationTokenSource CancellationTokenSource = new();
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
				[]
			);
			await ctxt.InitializeAsync();

			return await Run(ctxt);
		}
	}
}
