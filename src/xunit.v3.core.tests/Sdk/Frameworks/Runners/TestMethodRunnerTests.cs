using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestMethodRunnerTests
{
    [Fact]
    public static async void Messages()
    {
        var summary = new RunSummary { Total = 4, Failed = 2, Skipped = 1, Time = 21.12m };
        var messageBus = new SpyMessageBus();
        var testCase = Mocks.TestCase<ClassUnderTest>("Passing");
        var runner = TestableTestMethodRunner.Create(messageBus, new[] { testCase }, result: summary);

        var result = await runner.RunAsync();

        Assert.Equal(result.Total, summary.Total);
        Assert.Equal(result.Failed, summary.Failed);
        Assert.Equal(result.Skipped, summary.Skipped);
        Assert.Equal(result.Time, summary.Time);
        Assert.False(runner.TokenSource.IsCancellationRequested);
        Assert.Collection(messageBus.Messages,
            msg =>
            {
                var starting = Assert.IsAssignableFrom<ITestMethodStarting>(msg);
                Assert.Same(testCase.TestMethod.TestClass.TestCollection, starting.TestCollection);
                Assert.Equal("TestMethodRunnerTests+ClassUnderTest", starting.TestClass.Class.Name);
                Assert.Equal("Passing", starting.TestMethod.Method.Name);
            },
            msg =>
            {
                var finished = Assert.IsAssignableFrom<ITestMethodFinished>(msg);
                Assert.Same(testCase.TestMethod.TestClass.TestCollection, finished.TestCollection);
                Assert.Equal("TestMethodRunnerTests+ClassUnderTest", finished.TestClass.Class.Name);
                Assert.Equal("Passing", finished.TestMethod.Method.Name);
                Assert.Equal(21.12m, finished.ExecutionTime);
                Assert.Equal(4, finished.TestsRun);
                Assert.Equal(2, finished.TestsFailed);
                Assert.Equal(1, finished.TestsSkipped);
            }
        );
    }

    [Fact]
    public static async void FailureInQueueOfTestMethodStarting_DoesNotQueueTestMethodFinished_DoesNotRunTestCases()
    {
        var messages = new List<IMessageSinkMessage>();
        var messageBus = Substitute.For<IMessageBus>();
        messageBus.QueueMessage(null)
                  .ReturnsForAnyArgs(callInfo =>
                  {
                      var msg = callInfo.Arg<IMessageSinkMessage>();
                      messages.Add(msg);

                      if (msg is ITestMethodStarting)
                          throw new InvalidOperationException();

                      return true;
                  });
        var runner = TestableTestMethodRunner.Create(messageBus);

        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync());

        var starting = Assert.Single(messages);
        Assert.IsAssignableFrom<ITestMethodStarting>(starting);
        Assert.Empty(runner.TestCasesRun);
    }

    [Fact]
    public static async void RunTestCaseAsync_AggregatorIncludesPassedInExceptions()
    {
        var messageBus = new SpyMessageBus();
        var ex = new DivideByZeroException();
        var runner = TestableTestMethodRunner.Create(messageBus, aggregatorSeedException: ex);

        await runner.RunAsync();

        Assert.Same(ex, runner.RunTestCaseAsync_AggregatorResult);
        Assert.Empty(messageBus.Messages.OfType<ITestMethodCleanupFailure>());
    }

    [Fact]
    public static async void FailureInAfterTestMethodStarting_GivesErroredAggregatorToTestCaseRunner_NoCleanupFailureMessage()
    {
        var messageBus = new SpyMessageBus();
        var runner = TestableTestMethodRunner.Create(messageBus);
        var ex = new DivideByZeroException();
        runner.AfterTestMethodStarting_Callback = aggregator => aggregator.Add(ex);

        await runner.RunAsync();

        Assert.Same(ex, runner.RunTestCaseAsync_AggregatorResult);
        Assert.Empty(messageBus.Messages.OfType<ITestMethodCleanupFailure>());
    }

    [Fact]
    public static async void FailureInBeforeTestMethodFinished_ReportsCleanupFailure_DoesNotIncludeExceptionsFromAfterTestMethodStarting()
    {
        var messageBus = new SpyMessageBus();
        var testCases = new[] { Mocks.TestCase<TestAssemblyRunnerTests.RunAsync>("Messages") };
        var runner = TestableTestMethodRunner.Create(messageBus, testCases);
        var startingException = new DivideByZeroException();
        var finishedException = new InvalidOperationException();
        runner.AfterTestMethodStarting_Callback = aggregator => aggregator.Add(startingException);
        runner.BeforeTestMethodFinished_Callback = aggregator => aggregator.Add(finishedException);

        await runner.RunAsync();

        var cleanupFailure = Assert.Single(messageBus.Messages.OfType<ITestMethodCleanupFailure>());
        Assert.Same(testCases[0].TestMethod.TestClass.TestCollection, cleanupFailure.TestCollection);
        Assert.Equal(testCases, cleanupFailure.TestCases);
        Assert.Equal(typeof(InvalidOperationException).FullName, cleanupFailure.ExceptionTypes.Single());
    }

    [Fact]
    public static async void Cancellation_TestMethodStarting_DoesNotCallExtensibilityMethods()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestMethodStarting));
        var runner = TestableTestMethodRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
        Assert.False(runner.AfterTestMethodStarting_Called);
        Assert.False(runner.BeforeTestMethodFinished_Called);
    }

    [Fact]
    public static async void Cancellation_TestMethodFinished_CallsExtensibilityMethods()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestMethodFinished));
        var runner = TestableTestMethodRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
        Assert.True(runner.AfterTestMethodStarting_Called);
        Assert.True(runner.BeforeTestMethodFinished_Called);
    }

    [Fact]
    public static async void Cancellation_TestMethodCleanupFailure_SetsCancellationToken()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestMethodCleanupFailure));
        var runner = TestableTestMethodRunner.Create(messageBus);
        runner.BeforeTestMethodFinished_Callback = aggregator => aggregator.Add(new Exception());

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
    }

    [Fact]
    public static async void SignalingCancellationStopsRunningMethods()
    {
        var passing = Mocks.TestCase<ClassUnderTest>("Passing");
        var other = Mocks.TestCase<ClassUnderTest>("Other");
        var runner = TestableTestMethodRunner.Create(testCases: new[] { passing, other }, cancelInRunTestCaseAsync: true);

        await runner.RunAsync();

        var testCase = Assert.Single(runner.TestCasesRun);
        Assert.Same(passing, testCase);
    }

    class ClassUnderTest
    {
        [Fact]
        public void Passing() { }

        [Fact]
        public void Other() { }
    }

    class TestableTestMethodRunner : TestMethodRunner<ITestCase>
    {
        readonly bool cancelInRunTestCaseAsync;
        readonly RunSummary result;

        public bool AfterTestMethodStarting_Called;
        public Action<ExceptionAggregator> AfterTestMethodStarting_Callback = _ => { };
        public bool BeforeTestMethodFinished_Called;
        public Action<ExceptionAggregator> BeforeTestMethodFinished_Callback = _ => { };
        public Exception RunTestCaseAsync_AggregatorResult;
        public readonly CancellationTokenSource TokenSource;

        public List<ITestCase> TestCasesRun = new List<ITestCase>();

        TestableTestMethodRunner(ITestMethod testMethod,
                                 IReflectionTypeInfo @class,
                                 IReflectionMethodInfo method,
                                 IEnumerable<ITestCase> testCases,
                                 IMessageBus messageBus,
                                 ExceptionAggregator aggregator,
                                 CancellationTokenSource cancellationTokenSource,
                                 RunSummary result,
                                 bool cancelInRunTestCaseAsync)
            : base(testMethod, @class, method, testCases, messageBus, aggregator, cancellationTokenSource)
        {
            TokenSource = cancellationTokenSource;

            this.result = result;
            this.cancelInRunTestCaseAsync = cancelInRunTestCaseAsync;
        }

        public static TestableTestMethodRunner Create(IMessageBus messageBus = null,
                                                      ITestCase[] testCases = null,
                                                      RunSummary result = null,
                                                      Exception aggregatorSeedException = null,
                                                      bool cancelInRunTestCaseAsync = false)
        {
            if (testCases == null)
                testCases = new[] { Mocks.TestCase<ClassUnderTest>("Passing") };

            var firstTestCase = testCases.First();

            var aggregator = new ExceptionAggregator();
            if (aggregatorSeedException != null)
                aggregator.Add(aggregatorSeedException);

            return new TestableTestMethodRunner(
                firstTestCase.TestMethod,
                (IReflectionTypeInfo)firstTestCase.TestMethod.TestClass.Class,
                (IReflectionMethodInfo)firstTestCase.TestMethod.Method,
                testCases,
                messageBus ?? new SpyMessageBus(),
                aggregator,
                new CancellationTokenSource(),
                result ?? new RunSummary(),
                cancelInRunTestCaseAsync
            );
        }

        protected override void AfterTestMethodStarting()
        {
            AfterTestMethodStarting_Called = true;
            AfterTestMethodStarting_Callback(Aggregator);
        }

        protected override void BeforeTestMethodFinished()
        {
            BeforeTestMethodFinished_Called = true;
            BeforeTestMethodFinished_Callback(Aggregator);
        }

        protected override Task<RunSummary> RunTestCaseAsync(ITestCase testCase)
        {
            if (cancelInRunTestCaseAsync)
                CancellationTokenSource.Cancel();

            RunTestCaseAsync_AggregatorResult = Aggregator.ToException();
            TestCasesRun.Add(testCase);

            return Task.FromResult(result);
        }
    }
}
