using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestInvokerTests
{
    [Fact]
    public static async void Messages_StaticTestMethod()
    {
        var messageBus = new SpyMessageBus();
        var invoker = TestableTestInvoker.Create<NonDisposableClass>("StaticPassing", messageBus);

        await invoker.RunAsync();

        Assert.Empty(messageBus.Messages);
        Assert.True(invoker.OnTestExecuting_Called);
        Assert.True(invoker.OnTestExecuted_Called);
    }

    [Fact]
    public static async void Messages_NonStaticTestMethod_NoDispose()
    {
        var messageBus = new SpyMessageBus();
        var invoker = TestableTestInvoker.Create<NonDisposableClass>("Passing", messageBus, "Display Name");

        await invoker.RunAsync();

        Assert.Collection(messageBus.Messages,
            msg =>
            {
                var starting = Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg);
                Assert.Same(invoker.TestCase.TestCollection, starting.TestCollection);
                Assert.Same(invoker.TestCase, starting.TestCase);
                Assert.Equal("Display Name", starting.TestDisplayName);
            },
            msg =>
            {
                var finished = Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg);
                Assert.Same(invoker.TestCase.TestCollection, finished.TestCollection);
                Assert.Same(invoker.TestCase, finished.TestCase);
                Assert.Equal("Display Name", finished.TestDisplayName);
            }
        );
    }

    [Fact]
    public static async void Messages_NonStaticTestMethod_WithDispose()
    {
        var messageBus = new SpyMessageBus();
        var invoker = TestableTestInvoker.Create<DisposableClass>("Passing", messageBus, "Display Name");

        await invoker.RunAsync();

        Assert.Collection(messageBus.Messages,
            msg => Assert.IsAssignableFrom<ITestClassConstructionStarting>(msg),
            msg => Assert.IsAssignableFrom<ITestClassConstructionFinished>(msg),
            msg =>
            {
                var starting = Assert.IsAssignableFrom<ITestClassDisposeStarting>(msg);
                Assert.Same(invoker.TestCase.TestCollection, starting.TestCollection);
                Assert.Same(invoker.TestCase, starting.TestCase);
                Assert.Equal("Display Name", starting.TestDisplayName);
            },
            msg =>
            {
                var finished = Assert.IsAssignableFrom<ITestClassDisposeFinished>(msg);
                Assert.Same(invoker.TestCase.TestCollection, finished.TestCollection);
                Assert.Same(invoker.TestCase, finished.TestCase);
                Assert.Equal("Display Name", finished.TestDisplayName);
            }
        );
    }

    [Fact]
    public static async void Passing()
    {
        var messageBus = new SpyMessageBus();
        var invoker = TestableTestInvoker.Create<NonDisposableClass>("Passing", messageBus);

        var result = await invoker.RunAsync();

        Assert.NotEqual(0m, result);
        Assert.Null(invoker.Aggregator.ToException());
    }

    [Fact]
    public static async void Failing()
    {
        var messageBus = new SpyMessageBus();
        var invoker = TestableTestInvoker.Create<NonDisposableClass>("Failing", messageBus);

        var result = await invoker.RunAsync();

        Assert.NotEqual(0m, result);
        Assert.IsType<TrueException>(invoker.Aggregator.ToException());
    }

    [Fact]
    public static async void CancellationRequested_DoesNotInvokeTestMethod()
    {
        var messageBus = new SpyMessageBus();
        var invoker = TestableTestInvoker.Create<NonDisposableClass>("Failing", messageBus);
        invoker.TokenSource.Cancel();

        var result = await invoker.RunAsync();

        Assert.Equal(0m, result);
        Assert.Null(invoker.Aggregator.ToException());
        Assert.False(invoker.OnTestExecuting_Called);
        Assert.False(invoker.OnTestExecuted_Called);
    }

    [Theory]
    [InlineData("AsyncTask")]
    [InlineData("AsyncVoid")]
    public static async void TestCanBeAsync(string methodName)
    {
        var messageBus = new SpyMessageBus();
        var invoker = TestableTestInvoker.Create<NonDisposableClass>(methodName, messageBus);

        var result = await invoker.RunAsync();

        Assert.NotEqual(0m, result);
        Assert.IsType<TrueException>(invoker.Aggregator.ToException());
    }

    class NonDisposableClass
    {
        [Fact]
        public static void StaticPassing() { }

        [Fact]
        public void Passing() { }

        [Fact]
        public void Failing()
        {
            Assert.True(false);
        }

        [Fact]
        public async Task AsyncTask()
        {
            await Task.Delay(1);
            Assert.True(false);
        }

        [Fact]
        public async void AsyncVoid()
        {
            await Task.Delay(1);
            Assert.True(false);
        }
    }

    class DisposableClass : IDisposable
    {
        public void Dispose() { }

        [Fact]
        public void Passing() { }
    }

    class TestableTestInvoker : TestInvoker<ITestCase>
    {
        public readonly new ExceptionAggregator Aggregator;
        public bool OnTestExecuted_Called;
        public bool OnTestExecuting_Called;
        public readonly new ITestCase TestCase;
        public readonly CancellationTokenSource TokenSource;

        TestableTestInvoker(ITestCase testCase, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, string displayName, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            : base(testCase, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, displayName, aggregator, cancellationTokenSource)
        {
            TestCase = testCase;
            Aggregator = aggregator;
            TokenSource = cancellationTokenSource;
        }

        public static TestableTestInvoker Create<TClassUnderTest>(string methodName, IMessageBus messageBus, string displayName = null)
        {
            var testCase = Mocks.TestCase<TClassUnderTest>(methodName);

            return new TestableTestInvoker(
                testCase,
                messageBus,
                typeof(TClassUnderTest),
                new object[0],
                typeof(TClassUnderTest).GetMethod(methodName),
                new object[0],
                displayName,
                new ExceptionAggregator(),
                new CancellationTokenSource()
            );
        }

        protected override void OnTestExecuted()
        {
            OnTestExecuted_Called = true;
        }

        protected override void OnTestExecuting()
        {
            OnTestExecuting_Called = true;
        }
    }
}
