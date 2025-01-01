using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestMethodRunnerTests
{
	public class Messages
	{
		[Fact]
		public static async ValueTask Passing()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageBus.Messages,
				msg =>
				{
					var starting = Assert.IsAssignableFrom<ITestMethodStarting>(msg);
					verifyTestMethodMessage(starting);
					Assert.Equal("Passing", starting.MethodName);
					// Trait comes from an assembly-level trait attribute on this test assembly
					var trait = Assert.Single(starting.Traits);
					Assert.Equal("Assembly", trait.Key);
					var value = Assert.Single(trait.Value);
					Assert.Equal("Trait", value);
				},
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg),
				// Test method is invoked here
				msg => Assert.IsAssignableFrom<ITestClassDisposeStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestClassDisposeFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestPassed>(msg),
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => verifyTestMethodMessage(Assert.IsAssignableFrom<ITestMethodFinished>(msg))
			);

			static void verifyTestMethodMessage(ITestMethodMessage message)
			{
				Assert.Equal("assembly-id", message.AssemblyUniqueID);
				Assert.Equal("test-class-id", message.TestClassUniqueID);
				Assert.Equal("test-collection-id", message.TestCollectionUniqueID);
				Assert.Equal("test-method-id", message.TestMethodUniqueID);
			}
		}

		[Fact]
		public static async ValueTask StaticPassing()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.StaticPassing));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				// Test method is invoked here
				msg => Assert.IsAssignableFrom<ITestPassed>(msg),
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask Failed()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Failing));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
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
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaAttribute()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaAttribute));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg =>
				{
					var skipped = Assert.IsAssignableFrom<ITestSkipped>(msg);
					Assert.Equal("Don't run me", skipped.Reason);
				},
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaException()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaException));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
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
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaRegisteredException()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaRegisteredException));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
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
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg)
			);
		}

		[Fact]
		public static async ValueTask NotRun()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.ExplicitTest));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsAssignableFrom<ITestMethodStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestStarting>(msg),
				msg => Assert.IsAssignableFrom<ITestNotRun>(msg),
				msg => Assert.IsAssignableFrom<ITestFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestCaseFinished>(msg),
				msg => Assert.IsAssignableFrom<ITestMethodFinished>(msg)
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

	public class SelfExecution
	{
		[Fact]
		public async ValueTask SupportsSelfExecutingTestCases()
		{
			var testMethod = TestData.XunitTestMethod<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var testCase = new SelfExecutingTestCase(testMethod);
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			var skipped = Assert.Single(runner.MessageBus.Messages.OfType<ITestSkipped>());
			Assert.Equal("This is skipped via self-execution", skipped.Reason);
		}

		class ClassUnderTest
		{
			[Fact]
			public void Passing() { }
		}

		class SelfExecutingTestCase : XunitTestCase, ISelfExecutingXunitTestCase
		{
			[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
			public SelfExecutingTestCase()
			{ }

			public SelfExecutingTestCase(IXunitTestMethod testMethod) :
					base(testMethod, "Display Name", "Unique ID", @explicit: false)
			{ }

			public ValueTask<RunSummary> Run(
				ExplicitOption explicitOption,
				IMessageBus messageBus,
				object?[] constructorArguments,
				ExceptionAggregator aggregator,
				CancellationTokenSource cancellationTokenSource) =>
					new(XunitRunnerHelper.SkipTestCases(messageBus, cancellationTokenSource, [this], "This is skipped via self-execution"));
		}
	}

	class TestableXunitTestMethodRunner(IXunitTestCase testCase) :
		XunitTestMethodRunner
	{
		public readonly ExceptionAggregator Aggregator = new();
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public readonly List<string> Invocations = [];
		public readonly SpyMessageBus MessageBus = new();

		public ValueTask<RunSummary> RunAsync() =>
			Run(testCase.TestMethod, [testCase], ExplicitOption.Off, MessageBus, Aggregator, CancellationTokenSource, []);
	}
}
