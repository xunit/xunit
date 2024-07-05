using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestCaseRunnerBaseTests
{
	public class Messages
	{
		[Fact]
		public static async ValueTask Passing()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestCaseRunnerBase(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageBus.Messages,
				msg =>
				{
					var starting = Assert.IsType<_TestCaseStarting>(msg);
					verifyTestCaseMessage(starting);
					// Reading the assembly-level trait on the test project
					var trait = Assert.Single(starting.Traits);
					Assert.Equal("Assembly", trait.Key);
					var value = Assert.Single(trait.Value);
					Assert.Equal("Trait", value);
				},
				msg => Assert.IsType<_TestStarting>(msg),
				msg => Assert.IsType<_TestClassConstructionStarting>(msg),
				msg => Assert.IsType<_TestClassConstructionFinished>(msg),
				// Test method is invoked here
				msg => Assert.IsType<_TestClassDisposeStarting>(msg),
				msg => Assert.IsType<_TestClassDisposeFinished>(msg),
				msg => Assert.IsType<_TestPassed>(msg),
				msg => Assert.IsType<_TestFinished>(msg),
				msg => verifyTestCaseMessage(Assert.IsType<_TestCaseFinished>(msg))
			);

			static void verifyTestCaseMessage(_TestCaseMessage message)
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
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.StaticPassing));
			var runner = new TestableXunitTestCaseRunnerBase(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<_TestCaseStarting>(msg),
				msg => Assert.IsType<_TestStarting>(msg),
				// Test method is invoked here
				msg => Assert.IsType<_TestPassed>(msg),
				msg => Assert.IsType<_TestFinished>(msg),
				msg => Assert.IsType<_TestCaseFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask Failed()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Failing));
			var runner = new TestableXunitTestCaseRunnerBase(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<_TestCaseStarting>(msg),
				msg => Assert.IsType<_TestStarting>(msg),
				msg => Assert.IsType<_TestClassConstructionStarting>(msg),
				msg => Assert.IsType<_TestClassConstructionFinished>(msg),
				// Test method is invoked here
				msg => Assert.IsType<_TestClassDisposeStarting>(msg),
				msg => Assert.IsType<_TestClassDisposeFinished>(msg),
				msg =>
				{
					var failed = Assert.IsType<_TestFailed>(msg);
					Assert.Equal(-1, failed.ExceptionParentIndices.Single());
					Assert.Equal("Xunit.Sdk.TrueException", failed.ExceptionTypes.Single());
				},
				msg => Assert.IsType<_TestFinished>(msg),
				msg => Assert.IsType<_TestCaseFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaAttribute()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaAttribute));
			var runner = new TestableXunitTestCaseRunnerBase(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<_TestCaseStarting>(msg),
				msg => Assert.IsType<_TestStarting>(msg),
				msg =>
				{
					var skipped = Assert.IsType<_TestSkipped>(msg);
					Assert.Equal("Don't run me", skipped.Reason);
				},
				msg => Assert.IsType<_TestFinished>(msg),
				msg => Assert.IsType<_TestCaseFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaException()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaException));
			var runner = new TestableXunitTestCaseRunnerBase(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<_TestCaseStarting>(msg),
				msg => Assert.IsType<_TestStarting>(msg),
				msg => Assert.IsType<_TestClassConstructionStarting>(msg),
				msg => Assert.IsType<_TestClassConstructionFinished>(msg),
				// Test method is invoked here
				msg => Assert.IsType<_TestClassDisposeStarting>(msg),
				msg => Assert.IsType<_TestClassDisposeFinished>(msg),
				msg =>
				{
					var skipped = Assert.IsType<_TestSkipped>(msg);
					Assert.Equal("This isn't a good time", skipped.Reason);
				},
				msg => Assert.IsType<_TestFinished>(msg),
				msg => Assert.IsType<_TestCaseFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask NotRun()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.ExplicitTest));
			var runner = new TestableXunitTestCaseRunnerBase(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<_TestCaseStarting>(msg),
				msg => Assert.IsType<_TestStarting>(msg),
				msg => Assert.IsType<_TestNotRun>(msg),
				msg => Assert.IsType<_TestFinished>(msg),
				msg => Assert.IsType<_TestCaseFinished>(msg)
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

			[Fact(Explicit = true)]
			public void ExplicitTest() => Assert.Fail("Should not run");
		}
	}

	class TestableXunitTestCaseRunnerBase(IXunitTestCase? testCase = null) :
		XunitTestCaseRunnerBase<XunitTestCaseRunnerContext<IXunitTestCase>, IXunitTestCase>
	{
		public ExceptionAggregator Aggregator = new();
		public CancellationTokenSource CancellationTokenSource = new();
		public SpyMessageBus MessageBus = new();
		public IXunitTestCase TestCase = testCase ?? Mocks.XunitTestCase();

		public ValueTask<RunSummary> RunAsync() =>
			RunAsync(new(TestCase, MessageBus, Aggregator, CancellationTokenSource, TestCase.TestCaseDisplayName, TestCase.SkipReason, ExplicitOption.Off, [], []));
	}
}
