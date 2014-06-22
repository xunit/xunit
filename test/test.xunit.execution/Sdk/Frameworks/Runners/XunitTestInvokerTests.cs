﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

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
            Assert.Collection(messageBus.Messages,
                msg => Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg),  // From TestInvoker
                msg => Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg),  // From TestInvoker
                msg =>
                {
                    var beforeStarting = Assert.IsAssignableFrom<IBeforeTestStarting>(msg);
                    Assert.Same(invoker.TestCase.TestMethod.TestClass.TestCollection, beforeStarting.TestCollection);
                    Assert.Same(invoker.TestCase, beforeStarting.TestCase);
                    Assert.Equal("Display Name", beforeStarting.TestDisplayName);
                    Assert.Equal("SpyBeforeAfterTest", beforeStarting.AttributeName);
                },
                msg =>
                {
                    var beforeFinished = Assert.IsAssignableFrom<IBeforeTestFinished>(msg);
                    Assert.Same(invoker.TestCase.TestMethod.TestClass.TestCollection, beforeFinished.TestCollection);
                    Assert.Same(invoker.TestCase, beforeFinished.TestCase);
                    Assert.Equal("Display Name", beforeFinished.TestDisplayName);
                    Assert.Equal("SpyBeforeAfterTest", beforeFinished.AttributeName);
                },
                // Test method is invoked here; no directly observable message (tested below)
                msg =>
                {
                    var afterStarting = Assert.IsAssignableFrom<IAfterTestStarting>(msg);
                    Assert.Same(invoker.TestCase.TestMethod.TestClass.TestCollection, afterStarting.TestCollection);
                    Assert.Same(invoker.TestCase, afterStarting.TestCase);
                    Assert.Equal("Display Name", afterStarting.TestDisplayName);
                    Assert.Equal("SpyBeforeAfterTest", afterStarting.AttributeName);
                },
                msg =>
                {
                    var afterFinished = Assert.IsAssignableFrom<IAfterTestFinished>(msg);
                    Assert.Same(invoker.TestCase.TestMethod.TestClass.TestCollection, afterFinished.TestCollection);
                    Assert.Same(invoker.TestCase, afterFinished.TestCase);
                    Assert.Equal("Display Name", afterFinished.TestDisplayName);
                    Assert.Equal("SpyBeforeAfterTest", afterFinished.AttributeName);
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
            Assert.Collection(messageBus.Messages,
                msg => Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg),
                msg => Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg),
                msg => Assert.IsAssignableFrom<IBeforeTestStarting>(msg),
                msg => Assert.IsAssignableFrom<IBeforeTestFinished>(msg)
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
            Assert.Collection(aggEx.InnerExceptions,
                ex => Assert.IsType<TrueException>(ex),
                ex => Assert.IsType<SpyBeforeAfterTest.AfterException>(ex)
            );
            Assert.Collection(messageBus.Messages,
                msg => Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg),
                msg => Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg),
                msg => Assert.IsAssignableFrom<IBeforeTestStarting>(msg),
                msg => Assert.IsAssignableFrom<IBeforeTestFinished>(msg),
                msg => Assert.IsAssignableFrom<IAfterTestStarting>(msg),
                msg => Assert.IsAssignableFrom<IAfterTestFinished>(msg)
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

            Assert.Collection(messages,
                msg => Assert.Equal("Before #1", msg)
                // No cleanup for anything, so we had nothing run successfully
            );
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

            Assert.Collection(messages,
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

            Assert.Collection(messages,
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

            Assert.Collection(messages,
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

        public override void After(MethodInfo methodUnderTest)
        {
            messages.Add("After #" + identifier);
            base.After(methodUnderTest);
        }

        public override void Before(MethodInfo methodUnderTest)
        {
            messages.Add("Before #" + identifier);
            base.Before(methodUnderTest);
        }
    }

    class TestableXunitTestInvoker : XunitTestInvoker
    {
        readonly Action lambda;

        public readonly new ExceptionAggregator Aggregator;
        public readonly new IXunitTestCase TestCase;
        public readonly CancellationTokenSource TokenSource;

        TestableXunitTestInvoker(IXunitTestCase testCase, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, string displayName, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, Action lambda)
            : base(testCase, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, beforeAfterAttributes, displayName, aggregator, cancellationTokenSource)
        {
            this.lambda = lambda;

            TestCase = testCase;
            Aggregator = aggregator;
            TokenSource = cancellationTokenSource;
        }

        public static TestableXunitTestInvoker Create(IMessageBus messageBus = null, string displayName = null, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes = null, Action lambda = null)
        {
            return new TestableXunitTestInvoker(
                Mocks.XunitTestCase<ClassUnderTest>("Passing"),
                messageBus ?? new SpyMessageBus(),
                typeof(ClassUnderTest),
                new object[0],
                typeof(ClassUnderTest).GetMethod("Passing"),
                new object[0],
                beforeAfterAttributes ?? new List<BeforeAfterTestAttribute>(),
                displayName,
                new ExceptionAggregator(),
                new CancellationTokenSource(),
                lambda
            );
        }

        protected override Task InvokeTestMethodAsync(object testClassInstance)
        {
            if (lambda == null)
                return base.InvokeTestMethodAsync(testClassInstance);

            Aggregator.Run(lambda);
            return Task.FromResult(0);
        }

        class ClassUnderTest
        {
            [Fact]
            public void Passing() { }
        }
    }
}
