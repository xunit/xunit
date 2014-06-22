using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestCaseRunnerTests
{
    [Fact]
    public static async void Messages()
    {
        var summary = new RunSummary { Total = 4, Failed = 2, Skipped = 1, Time = 21.12m };
        var messageBus = new SpyMessageBus();
        var runner = TestableTestCaseRunner.Create(messageBus, summary);

        var result = await runner.RunAsync();

        Assert.Same(result, summary);
        Assert.False(runner.TokenSource.IsCancellationRequested);
        Assert.Collection(messageBus.Messages,
            msg =>
            {
                var testCaseStarting = Assert.IsAssignableFrom<ITestCaseStarting>(msg);
                Assert.Same(runner.TestCase.TestMethod.TestClass.TestCollection, testCaseStarting.TestCollection);
                Assert.Same(runner.TestCase, testCaseStarting.TestCase);
            },
            msg =>
            {
                var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(msg);
                Assert.Same(runner.TestCase.TestMethod.TestClass.TestCollection, testCaseFinished.TestCollection);
                Assert.Same(runner.TestCase, testCaseFinished.TestCase);
                Assert.Equal(21.12m, testCaseFinished.ExecutionTime);
                Assert.Equal(4, testCaseFinished.TestsRun);
                Assert.Equal(2, testCaseFinished.TestsFailed);
                Assert.Equal(1, testCaseFinished.TestsSkipped);
            }
        );
    }

    [Fact]
    public static async void Cancellation_TestCaseStarting_CallsOuterMethodsOnly()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestCaseStarting));
        var runner = TestableTestCaseRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.OnTestCaseStarting_Called);
        Assert.False(runner.OnTestCaseStarted_Called);
        Assert.False(runner.OnTestCaseFinishing_Called);
        Assert.True(runner.OnTestCaseFinished_Called);
    }

    [Fact]
    public static async void Cancellation_TestCaseFinished_CallsOuterAndInnerMethods()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestCaseFinished));
        var runner = TestableTestCaseRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.OnTestCaseStarting_Called);
        Assert.True(runner.OnTestCaseStarted_Called);
        Assert.True(runner.OnTestCaseFinishing_Called);
        Assert.True(runner.OnTestCaseFinished_Called);
    }

    [Theory]
    [InlineData(typeof(ITestCaseStarting))]
    [InlineData(typeof(ITestCaseFinished))]
    public static async void Cancellation_TriggersCancellationTokenSource(Type messageTypeToCancelOn)
    {
        var messageBus = new SpyMessageBus(msg => !(messageTypeToCancelOn.IsAssignableFrom(msg.GetType())));
        var runner = TestableTestCaseRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
    }

    class TestableTestCaseRunner : TestCaseRunner<ITestCase>
    {
        readonly RunSummary result;

        public bool OnTestCaseFinished_Called;
        public bool OnTestCaseFinishing_Called;
        public bool OnTestCaseStarted_Called;
        public bool OnTestCaseStarting_Called;
        public readonly new ITestCase TestCase;
        public CancellationTokenSource TokenSource;

        TestableTestCaseRunner(ITestCase testCase, IMessageBus messageBus, CancellationTokenSource tokenSource, RunSummary result)
            : base(testCase, messageBus, new ExceptionAggregator(), tokenSource)
        {
            this.result = result;

            TestCase = testCase;
            TokenSource = tokenSource;
        }

        public static TestableTestCaseRunner Create(IMessageBus messageBus, RunSummary result = null)
        {
            return new TestableTestCaseRunner(
                Mocks.TestCase<Object>("ToString"),
                messageBus,
                new CancellationTokenSource(),
                result ?? new RunSummary()
            );
        }

        protected override void OnTestCaseFinished()
        {
            OnTestCaseFinished_Called = true;
        }

        protected override void OnTestCaseFinishing()
        {
            OnTestCaseFinishing_Called = true;
        }

        protected override void OnTestCaseStarted()
        {
            OnTestCaseStarted_Called = true;
        }

        protected override void OnTestCaseStarting()
        {
            OnTestCaseStarting_Called = true;
        }

        protected override Task<RunSummary> RunTestAsync()
        {
            return Task.FromResult(result);
        }
    }
}
