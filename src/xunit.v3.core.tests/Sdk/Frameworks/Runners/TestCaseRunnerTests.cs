using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
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
        var runner = TestableTestCaseRunner.Create(messageBus, result: summary);

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
    public static async void FailureInQueueOfTestCaseStarting_DoesNotQueueTestCaseFinished_DoesNotRunTests()
    {
        var messages = new List<IMessageSinkMessage>();
        var messageBus = Substitute.For<IMessageBus>();
        messageBus.QueueMessage(null)
                  .ReturnsForAnyArgs(callInfo =>
                  {
                      var msg = callInfo.Arg<IMessageSinkMessage>();
                      messages.Add(msg);

                      if (msg is ITestCaseStarting)
                          throw new InvalidOperationException();

                      return true;
                  });
        var runner = TestableTestCaseRunner.Create(messageBus);

        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync());

        var starting = Assert.Single(messages);
        Assert.IsAssignableFrom<ITestCaseStarting>(starting);
        Assert.False(runner.RunTestAsync_Called);
    }

    [Fact]
    public static async void RunTestAsync_AggregatorIncludesPassedInExceptions()
    {
        var messageBus = new SpyMessageBus();
        var ex = new DivideByZeroException();
        var runner = TestableTestCaseRunner.Create(messageBus, aggregatorSeedException: ex);

        await runner.RunAsync();

        Assert.Same(ex, runner.RunTestAsync_AggregatorResult);
        Assert.Empty(messageBus.Messages.OfType<ITestCaseCleanupFailure>());
    }

    [Fact]
    public static async void FailureInAfterTestCaseStarting_GivesErroredAggregatorToTestRunner_NoCleanupFailureMessage()
    {
        var messageBus = new SpyMessageBus();
        var runner = TestableTestCaseRunner.Create(messageBus);
        var ex = new DivideByZeroException();
        runner.AfterTestCaseStarting_Callback = aggregator => aggregator.Add(ex);

        await runner.RunAsync();

        Assert.Same(ex, runner.RunTestAsync_AggregatorResult);
        Assert.Empty(messageBus.Messages.OfType<ITestCaseCleanupFailure>());
    }

    [Fact]
    public static async void FailureInBeforeTestCaseFinished_ReportsCleanupFailure_DoesNotIncludeExceptionsFromAfterTestCaseStarting()
    {
        var messageBus = new SpyMessageBus();
        var testCase = Mocks.TestCase<TestAssemblyRunnerTests.RunAsync>("Messages");
        var runner = TestableTestCaseRunner.Create(messageBus, testCase);
        var startingException = new DivideByZeroException();
        var finishedException = new InvalidOperationException();
        runner.AfterTestCaseStarting_Callback = aggregator => aggregator.Add(startingException);
        runner.BeforeTestCaseFinished_Callback = aggregator => aggregator.Add(finishedException);

        await runner.RunAsync();

        var cleanupFailure = Assert.Single(messageBus.Messages.OfType<ITestCaseCleanupFailure>());
        Assert.Same(testCase, cleanupFailure.TestCase);
        Assert.Equal(new[] { testCase }, cleanupFailure.TestCases);
        Assert.Equal(typeof(InvalidOperationException).FullName, cleanupFailure.ExceptionTypes.Single());
    }

    [Fact]
    public static async void Cancellation_TestCaseStarting_DoesNotCallExtensibilityMethods()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestCaseStarting));
        var runner = TestableTestCaseRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.False(runner.AfterTestCaseStarting_Called);
        Assert.False(runner.BeforeTestCaseFinished_Called);
    }

    [Fact]
    public static async void Cancellation_TestCaseFinished_CallsExtensibilityMethods()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestCaseFinished));
        var runner = TestableTestCaseRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.AfterTestCaseStarting_Called);
        Assert.True(runner.BeforeTestCaseFinished_Called);
    }

    [Fact]
    public static async void Cancellation_TestClassCleanupFailure_SetsCancellationToken()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestCaseCleanupFailure));
        var runner = TestableTestCaseRunner.Create(messageBus);
        runner.BeforeTestCaseFinished_Callback = aggregator => aggregator.Add(new Exception());

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
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

        public Action<ExceptionAggregator> AfterTestCaseStarting_Callback = _ => { };
        public bool AfterTestCaseStarting_Called;
        public Action<ExceptionAggregator> BeforeTestCaseFinished_Callback = _ => { };
        public bool BeforeTestCaseFinished_Called;
        public Exception RunTestAsync_AggregatorResult;
        public bool RunTestAsync_Called;
        public readonly new ITestCase TestCase;
        public CancellationTokenSource TokenSource;

        TestableTestCaseRunner(ITestCase testCase, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource tokenSource, RunSummary result)
            : base(testCase, messageBus, aggregator, tokenSource)
        {
            this.result = result;

            TestCase = testCase;
            TokenSource = tokenSource;
        }

        public static TestableTestCaseRunner Create(IMessageBus messageBus, ITestCase testCase = null, RunSummary result = null, Exception aggregatorSeedException = null)
        {
            var aggregator = new ExceptionAggregator();
            if (aggregatorSeedException != null)
                aggregator.Add(aggregatorSeedException);

            return new TestableTestCaseRunner(
                testCase ?? Mocks.TestCase<object>("ToString"),
                messageBus,
                aggregator,
                new CancellationTokenSource(),
                result ?? new RunSummary()
            );
        }

        protected override Task AfterTestCaseStartingAsync()
        {
            AfterTestCaseStarting_Called = true;
            AfterTestCaseStarting_Callback(Aggregator);
            return Task.FromResult(0);
        }

        protected override Task BeforeTestCaseFinishedAsync()
        {
            BeforeTestCaseFinished_Called = true;
            BeforeTestCaseFinished_Callback(Aggregator);
            return Task.FromResult(0);
        }

        protected override Task<RunSummary> RunTestAsync()
        {
            RunTestAsync_AggregatorResult = Aggregator.ToException();
            RunTestAsync_Called = true;

            return Task.FromResult(result);
        }
    }
}
