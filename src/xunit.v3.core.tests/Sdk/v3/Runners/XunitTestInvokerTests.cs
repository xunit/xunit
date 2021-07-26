using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestInvokerTests
{
	public class Messages
	{
		[Fact]
		public static async void Success()
		{
			var messageBus = new SpyMessageBus();
			var attribute = new SpyBeforeAfterTest();
			var invoker = TestableXunitTestInvoker.Create(messageBus, "Display Name", new List<BeforeAfterTestAttribute> { attribute });

			await invoker.RunAsync();

			Assert.Null(invoker.Aggregator.ToException());
			Assert.Collection(
				messageBus.Messages,
				msg => Assert.IsType<_TestClassConstructionStarting>(msg),  // From TestInvoker
				msg => Assert.IsType<_TestClassConstructionFinished>(msg),  // From TestInvoker
				msg =>
				{
					var beforeStarting = Assert.IsType<_BeforeTestStarting>(msg);
					Assert.Equal("assembly-id", beforeStarting.AssemblyUniqueID);
					Assert.Equal("SpyBeforeAfterTest", beforeStarting.AttributeName);
					Assert.Equal("case-id", beforeStarting.TestCaseUniqueID);
					Assert.Equal("class-id", beforeStarting.TestClassUniqueID);
					Assert.Equal("collection-id", beforeStarting.TestCollectionUniqueID);
					Assert.Equal("method-id", beforeStarting.TestMethodUniqueID);
					Assert.Equal("test-id", beforeStarting.TestUniqueID);
				},
				msg =>
				{
					var beforeFinished = Assert.IsType<_BeforeTestFinished>(msg);
					Assert.Equal("assembly-id", beforeFinished.AssemblyUniqueID);
					Assert.Equal("SpyBeforeAfterTest", beforeFinished.AttributeName);
					Assert.Equal("case-id", beforeFinished.TestCaseUniqueID);
					Assert.Equal("class-id", beforeFinished.TestClassUniqueID);
					Assert.Equal("collection-id", beforeFinished.TestCollectionUniqueID);
					Assert.Equal("method-id", beforeFinished.TestMethodUniqueID);
					Assert.Equal("test-id", beforeFinished.TestUniqueID);
				},
				// Test method is invoked here; no directly observable message (tested below)
				msg =>
				{
					var afterStarting = Assert.IsType<_AfterTestStarting>(msg);
					Assert.Equal("assembly-id", afterStarting.AssemblyUniqueID);
					Assert.Equal("SpyBeforeAfterTest", afterStarting.AttributeName);
					Assert.Equal("case-id", afterStarting.TestCaseUniqueID);
					Assert.Equal("class-id", afterStarting.TestClassUniqueID);
					Assert.Equal("collection-id", afterStarting.TestCollectionUniqueID);
					Assert.Equal("method-id", afterStarting.TestMethodUniqueID);
					Assert.Equal("test-id", afterStarting.TestUniqueID);
				},
				msg =>
				{
					var afterFinished = Assert.IsType<_AfterTestFinished>(msg);
					Assert.Equal("assembly-id", afterFinished.AssemblyUniqueID);
					Assert.Equal("SpyBeforeAfterTest", afterFinished.AttributeName);
					Assert.Equal("case-id", afterFinished.TestCaseUniqueID);
					Assert.Equal("class-id", afterFinished.TestClassUniqueID);
					Assert.Equal("collection-id", afterFinished.TestCollectionUniqueID);
					Assert.Equal("method-id", afterFinished.TestMethodUniqueID);
					Assert.Equal("test-id", afterFinished.TestUniqueID);
				}
			);
		}

		[Fact]
		public static async void FailedBefore()
		{
			var messageBus = new SpyMessageBus();
			var attribute = new SpyBeforeAfterTest { ThrowInBefore = true };
			var invoker = TestableXunitTestInvoker.Create(messageBus, "Display Name", new List<BeforeAfterTestAttribute> { attribute }, lambda: () => Assert.True(false));

			await invoker.RunAsync();

			Assert.IsType<SpyBeforeAfterTest.BeforeException>(invoker.Aggregator.ToException());
			Assert.Collection(
				messageBus.Messages,
				msg => Assert.IsType<_TestClassConstructionStarting>(msg),
				msg => Assert.IsType<_TestClassConstructionFinished>(msg),
				msg => Assert.IsType<_BeforeTestStarting>(msg),
				msg => Assert.IsType<_BeforeTestFinished>(msg)
			);
		}

		[Fact]
		public static async void FailedAfter()
		{
			var messageBus = new SpyMessageBus();
			var attribute = new SpyBeforeAfterTest { ThrowInAfter = true };
			var invoker = TestableXunitTestInvoker.Create(messageBus, "Display Name", new List<BeforeAfterTestAttribute> { attribute }, lambda: () => Assert.True(false));

			await invoker.RunAsync();

			var aggEx = Assert.IsType<AggregateException>(invoker.Aggregator.ToException());
			Assert.Collection(
				aggEx.InnerExceptions,
				ex => Assert.IsType<TrueException>(ex),
				ex => Assert.IsType<SpyBeforeAfterTest.AfterException>(ex)
			);
			Assert.Collection(
				messageBus.Messages,
				msg => Assert.IsType<_TestClassConstructionStarting>(msg),
				msg => Assert.IsType<_TestClassConstructionFinished>(msg),
				msg => Assert.IsType<_BeforeTestStarting>(msg),
				msg => Assert.IsType<_BeforeTestFinished>(msg),
				msg => Assert.IsType<_AfterTestStarting>(msg),
				msg => Assert.IsType<_AfterTestFinished>(msg)
			);
		}
	}

	public class ExecutionOrder
	{
		[Fact]
		public static async void Successful()
		{
			var messages = new List<string>();
			var attribute1 = new RecordingBeforeAfter(messages, 1);
			var attribute2 = new RecordingBeforeAfter(messages, 2);
			var invoker = TestableXunitTestInvoker.Create(
				beforeAfterAttributes: new List<BeforeAfterTestAttribute> { attribute1, attribute2 },
				lambda: () => messages.Add("Test method invocation")
			);

			await invoker.RunAsync();

			Assert.Collection(messages,
				msg => Assert.Equal("Before #1", msg),
				msg => Assert.Equal("Before #2", msg),
				msg => Assert.Equal("Test method invocation", msg),
				msg => Assert.Equal("After #2", msg),
				msg => Assert.Equal("After #1", msg)
			);
		}

		[Fact]
		public static async void FailingBefore_First()
		{
			var messages = new List<string>();
			var attribute1 = new RecordingBeforeAfter(messages, 1) { ThrowInBefore = true };
			var attribute2 = new RecordingBeforeAfter(messages, 2);
			var invoker = TestableXunitTestInvoker.Create(
				beforeAfterAttributes: new List<BeforeAfterTestAttribute> { attribute1, attribute2 },
				lambda: () => messages.Add("Test method invocation")
			);

			await invoker.RunAsync();

			var msg = Assert.Single(messages);
			Assert.Equal("Before #1", msg);
			// No cleanup for anything, so we had nothing run successfully
		}

		[Fact]
		public static async void FailingBefore_Second()
		{
			var messages = new List<string>();
			var attribute1 = new RecordingBeforeAfter(messages, 1);
			var attribute2 = new RecordingBeforeAfter(messages, 2) { ThrowInBefore = true };
			var invoker = TestableXunitTestInvoker.Create(
				beforeAfterAttributes: new List<BeforeAfterTestAttribute> { attribute1, attribute2 },
				lambda: () => messages.Add("Test method invocation")
			);

			await invoker.RunAsync();

			Assert.Collection(messages,
				msg => Assert.Equal("Before #1", msg),
				msg => Assert.Equal("Before #2", msg),
				// No cleanup for #2, since it threw
				msg => Assert.Equal("After #1", msg)
			);
		}

		[Fact]
		public static async void FailingAfter_First()
		{
			var messages = new List<string>();
			var attribute1 = new RecordingBeforeAfter(messages, 1) { ThrowInAfter = true };
			var attribute2 = new RecordingBeforeAfter(messages, 2);
			var invoker = TestableXunitTestInvoker.Create(
				beforeAfterAttributes: new List<BeforeAfterTestAttribute> { attribute1, attribute2 },
				lambda: () => messages.Add("Test method invocation")
			);

			await invoker.RunAsync();

			Assert.Collection(
				messages,
				msg => Assert.Equal("Before #1", msg),
				msg => Assert.Equal("Before #2", msg),
				msg => Assert.Equal("Test method invocation", msg),
				msg => Assert.Equal("After #2", msg),
				msg => Assert.Equal("After #1", msg)
			);
		}

		[Fact]
		public static async void FailingAfter_Second()
		{
			var messages = new List<string>();
			var attribute1 = new RecordingBeforeAfter(messages, 1);
			var attribute2 = new RecordingBeforeAfter(messages, 2) { ThrowInAfter = true };
			var invoker = TestableXunitTestInvoker.Create(
				beforeAfterAttributes: new List<BeforeAfterTestAttribute> { attribute1, attribute2 },
				lambda: () => messages.Add("Test method invocation")
			);

			await invoker.RunAsync();

			Assert.Collection(
				messages,
				msg => Assert.Equal("Before #1", msg),
				msg => Assert.Equal("Before #2", msg),
				msg => Assert.Equal("Test method invocation", msg),
				msg => Assert.Equal("After #2", msg),
				msg => Assert.Equal("After #1", msg)
			);
		}

		[Fact]
		public static async void FailingTest()
		{
			var messages = new List<string>();
			var attribute1 = new RecordingBeforeAfter(messages, 1);
			var attribute2 = new RecordingBeforeAfter(messages, 2);
			var invoker = TestableXunitTestInvoker.Create(
				beforeAfterAttributes: new List<BeforeAfterTestAttribute> { attribute1, attribute2 },
				lambda: () => { messages.Add("Test method invocation"); Assert.True(false); }
			);

			await invoker.RunAsync();

			Assert.Collection(
				messages,
				msg => Assert.Equal("Before #1", msg),
				msg => Assert.Equal("Before #2", msg),
				msg => Assert.Equal("Test method invocation", msg),
				msg => Assert.Equal("After #2", msg),
				msg => Assert.Equal("After #1", msg)
			);
		}
	}

	class RecordingBeforeAfter : SpyBeforeAfterTest
	{
		private readonly int identifier;
		private readonly List<string> messages;

		public RecordingBeforeAfter(List<string> messages, int identifier)
		{
			this.messages = messages;
			this.identifier = identifier;
		}

		public override void After(MethodInfo methodUnderTest, _ITest test)
		{
			messages.Add("After #" + identifier);
			base.After(methodUnderTest, test);
		}

		public override void Before(MethodInfo methodUnderTest, _ITest test)
		{
			messages.Add("Before #" + identifier);
			base.Before(methodUnderTest, test);
		}
	}

	class TestableXunitTestInvoker : XunitTestInvoker
	{
		readonly Action? lambda;

		public readonly new ExceptionAggregator Aggregator;
		public readonly new IXunitTestCase TestCase;
		public readonly CancellationTokenSource TokenSource;

		TestableXunitTestInvoker(
			_ITest test,
			IMessageBus messageBus,
			Type testClass,
			object?[] constructorArguments,
			MethodInfo testMethod,
			object?[]? testMethodArguments,
			IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource,
			Action? lambda) :
				base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, beforeAfterAttributes, aggregator, null, cancellationTokenSource)
		{
			this.lambda = lambda;

			TestCase = (IXunitTestCase)test.TestCase;
			Aggregator = aggregator;
			TokenSource = cancellationTokenSource;
		}

		public static TestableXunitTestInvoker Create(
			IMessageBus? messageBus = null,
			string displayName = "MockDisplayName",
			IReadOnlyList<BeforeAfterTestAttribute>? beforeAfterAttributes = null,
			Action? lambda = null)
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>("Passing", uniqueID: "case-id");
			var test = Mocks.Test(testCase, displayName, "test-id");

			return new TestableXunitTestInvoker(
				test,
				messageBus ?? new SpyMessageBus(),
				typeof(ClassUnderTest),
				new object[0],
				typeof(ClassUnderTest).GetMethod(nameof(ClassUnderTest.Passing))!,
				new object[0],
				beforeAfterAttributes ?? new List<BeforeAfterTestAttribute>(),
				new ExceptionAggregator(),
				new CancellationTokenSource(),
				lambda
			);
		}

		protected override Task InvokeTestMethodAsync(object? testClassInstance)
		{
			if (lambda == null)
				return base.InvokeTestMethodAsync(testClassInstance);

			Aggregator.Run(lambda);
			return Task.CompletedTask;
		}

		class ClassUnderTest
		{
			[Fact]
			public void Passing() { }
		}
	}
}
