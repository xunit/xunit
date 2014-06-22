using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestRunnerTests
{
    [Fact]
    public static async void Messages()
    {
        var messageBus = new SpyMessageBus();
        var runner = TestableTestRunner.Create(messageBus, "Display Name", runTime: 21.12m);

        var result = await runner.RunAsync();

        Assert.Equal(21.12m, result.Time);
        Assert.False(runner.TokenSource.IsCancellationRequested);
        Assert.Collection(messageBus.Messages,
            msg =>
            {
                var testStarting = Assert.IsAssignableFrom<ITestStarting>(msg);
                Assert.Same(runner.TestCase.TestMethod.TestClass.TestCollection, testStarting.TestCollection);
                Assert.Same(runner.TestCase, testStarting.TestCase);
                Assert.Equal("Display Name", testStarting.TestDisplayName);
            },
            msg => { },  // Pass/fail/skip, will be tested elsewhere
            msg =>
            {
                var testFinished = Assert.IsAssignableFrom<ITestFinished>(msg);
                Assert.Same(runner.TestCase.TestMethod.TestClass.TestCollection, testFinished.TestCollection);
                Assert.Same(runner.TestCase, testFinished.TestCase);
                Assert.Equal("Display Name", testFinished.TestDisplayName);
                Assert.Equal(21.12m, testFinished.ExecutionTime);
                Assert.Empty(testFinished.Output);
            }
        );
    }

    [Fact]
    public static async void Passing()
    {
        var messageBus = new SpyMessageBus();
        var runner = TestableTestRunner.Create(messageBus, "Display Name", runTime: 21.12m);

        var result = await runner.RunAsync();

        // Direct run summary
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
        Assert.Equal(0, result.Skipped);
        Assert.Equal(21.12m, result.Time);
        // Pass message
        var passed = messageBus.Messages.OfType<ITestPassed>().Single();
        Assert.Same(runner.TestCase.TestMethod.TestClass.TestCollection, passed.TestCollection);
        Assert.Same(runner.TestCase, passed.TestCase);
        Assert.Equal("Display Name", passed.TestDisplayName);
        Assert.Equal(21.12m, passed.ExecutionTime);
        Assert.Empty(passed.Output);
    }

    [Fact]
    public static async void Failing()
    {
        var messageBus = new SpyMessageBus();
        var runner = TestableTestRunner.Create(messageBus, "Display Name", runTime: 21.12m, lambda: () => Assert.True(false));

        var result = await runner.RunAsync();

        // Direct run summary
        Assert.Equal(1, result.Total);
        Assert.Equal(1, result.Failed);
        Assert.Equal(0, result.Skipped);
        Assert.Equal(21.12m, result.Time);
        // Fail message
        var failed = messageBus.Messages.OfType<ITestFailed>().Single();
        Assert.Same(runner.TestCase.TestMethod.TestClass.TestCollection, failed.TestCollection);
        Assert.Same(runner.TestCase, failed.TestCase);
        Assert.Equal("Display Name", failed.TestDisplayName);
        Assert.Equal(21.12m, failed.ExecutionTime);
        Assert.Empty(failed.Output);
        Assert.Equal("Xunit.Sdk.TrueException", failed.ExceptionTypes.Single());
    }

    [Fact]
    public static async void Skipping()
    {
        var messageBus = new SpyMessageBus();
        var runner = TestableTestRunner.Create(messageBus, "Display Name", skipReason: "Please don't run me", runTime: 21.12m, lambda: () => Assert.True(false));

        var result = await runner.RunAsync();

        // Direct run summary
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
        Assert.Equal(1, result.Skipped);
        Assert.Equal(0m, result.Time);
        // Skip message
        var failed = messageBus.Messages.OfType<ITestSkipped>().Single();
        Assert.Same(runner.TestCase.TestMethod.TestClass.TestCollection, failed.TestCollection);
        Assert.Same(runner.TestCase, failed.TestCase);
        Assert.Equal("Display Name", failed.TestDisplayName);
        Assert.Equal(0m, failed.ExecutionTime);
        Assert.Empty(failed.Output);
        Assert.Equal("Please don't run me", failed.Reason);
    }

    [Fact]
    public static async void Cancellation_TestStarting_CallsOuterMethodsOnly()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestStarting));
        var runner = TestableTestRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.OnTestStarting_Called);
        Assert.False(runner.OnTestStarted_Called);
        Assert.False(runner.OnTestFinishing_Called);
        Assert.True(runner.OnTestFinished_Called);
    }

    [Theory]
    [InlineData(typeof(ITestPassed), true, null)]
    [InlineData(typeof(ITestFailed), false, null)]
    [InlineData(typeof(ITestSkipped), false, "Please skip me")]
    [InlineData(typeof(ITestFinished), true, null)]
    public static async void Cancellation_AllOthers_CallsOuterAndInnerMethods(Type messageTypeToCancelOn, bool shouldTestPass, string skipReason = null)
    {
        var messageBus = new SpyMessageBus(msg => !(messageTypeToCancelOn.IsAssignableFrom(msg.GetType())));
        var runner = TestableTestRunner.Create(messageBus, skipReason: skipReason, lambda: () => Assert.True(shouldTestPass));

        await runner.RunAsync();

        Assert.True(runner.OnTestStarting_Called);
        Assert.True(runner.OnTestStarted_Called);
        Assert.True(runner.OnTestFinishing_Called);
        Assert.True(runner.OnTestFinished_Called);
    }

    [Theory]
    [InlineData(typeof(ITestStarting), true, null)]
    [InlineData(typeof(ITestPassed), true, null)]
    [InlineData(typeof(ITestFailed), false, null)]
    [InlineData(typeof(ITestSkipped), false, "Please skip me")]
    [InlineData(typeof(ITestFinished), true, null)]
    public static async void Cancellation_TriggersCancellationTokenSource(Type messageTypeToCancelOn, bool shouldTestPass, string skipReason = null)
    {
        var messageBus = new SpyMessageBus(msg => !(messageTypeToCancelOn.IsAssignableFrom(msg.GetType())));
        var runner = TestableTestRunner.Create(messageBus, skipReason: skipReason, lambda: () => Assert.True(shouldTestPass));

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
    }

    class TestableTestRunner : TestRunner<ITestCase>
    {
        readonly Action lambda;
        readonly decimal runTime;

        public bool OnTestFinished_Called;
        public bool OnTestFinishing_Called;
        public bool OnTestStarted_Called;
        public bool OnTestStarting_Called;
        public readonly new ITestCase TestCase;
        public CancellationTokenSource TokenSource;

        TestableTestRunner(ITestCase testCase, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, string displayName, string skipReason, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, decimal runTime, Action lambda)
            : base(testCase, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, displayName, skipReason, aggregator, cancellationTokenSource)
        {
            TestCase = testCase;
            TokenSource = cancellationTokenSource;

            this.runTime = runTime;
            this.lambda = lambda;
        }

        public static TestableTestRunner Create(IMessageBus messageBus, string displayName = null, string skipReason = null, decimal runTime = 0m, Action lambda = null)
        {
            return new TestableTestRunner(Mocks.TestCase<Object>("ToString"), messageBus, typeof(Object), new object[0], typeof(Object).GetMethod("ToString"), new object[0], displayName, skipReason, new ExceptionAggregator(), new CancellationTokenSource(), runTime, lambda);
        }

        protected override void OnTestFinished()
        {
            OnTestFinished_Called = true;
        }

        protected override void OnTestFinishing()
        {
            OnTestFinishing_Called = true;
        }

        protected override void OnTestStarted()
        {
            OnTestStarted_Called = true;
        }

        protected override void OnTestStarting()
        {
            OnTestStarting_Called = true;
        }

        protected override Task<decimal> InvokeTestAsync(ExceptionAggregator aggregator)
        {
            if (lambda != null)
                aggregator.Run(lambda);

            return Task.FromResult(runTime);
        }
    }
}
