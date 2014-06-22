using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public static async void Cancellation_TestMethodStarting_CallsOuterMethodsOnly()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestMethodStarting));
        var runner = TestableTestMethodRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
        Assert.True(runner.OnTestMethodStarting_Called);
        Assert.False(runner.OnTestMethodStarted_Called);
        Assert.False(runner.OnTestMethodFinishing_Called);
        Assert.True(runner.OnTestMethodFinished_Called);
    }

    [Fact]
    public static async void Cancellation_TestMethodFinished_CallsOuterAndInnerMethods()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestMethodFinished));
        var runner = TestableTestMethodRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
        Assert.True(runner.OnTestMethodStarting_Called);
        Assert.True(runner.OnTestMethodStarted_Called);
        Assert.True(runner.OnTestMethodFinishing_Called);
        Assert.True(runner.OnTestMethodFinished_Called);
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

        public bool OnTestMethodFinished_Called;
        public bool OnTestMethodFinishing_Called;
        public bool OnTestMethodStarted_Called;
        public bool OnTestMethodStarting_Called;
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
                                                      bool cancelInRunTestCaseAsync = false)
        {
            if (testCases == null)
                testCases = new[] { Mocks.TestCase<ClassUnderTest>("Passing") };

            var firstTestCase = testCases.First();

            return new TestableTestMethodRunner(
                firstTestCase.TestMethod,
                (IReflectionTypeInfo)firstTestCase.TestMethod.TestClass.Class,
                (IReflectionMethodInfo)firstTestCase.TestMethod.Method,
                testCases,
                messageBus ?? new SpyMessageBus(),
                new ExceptionAggregator(),
                new CancellationTokenSource(),
                result ?? new RunSummary(),
                cancelInRunTestCaseAsync
            );
        }

        protected override void OnTestMethodFinished()
        {
            OnTestMethodFinished_Called = true;
        }

        protected override void OnTestMethodFinishing()
        {
            OnTestMethodFinishing_Called = true;
        }

        protected override void OnTestMethodStarted()
        {
            OnTestMethodStarted_Called = true;
        }

        protected override void OnTestMethodStarting()
        {
            OnTestMethodStarting_Called = true;
        }

        protected override Task<RunSummary> RunTestCaseAsync(ITestCase testCase)
        {
            if (cancelInRunTestCaseAsync)
                CancellationTokenSource.Cancel();

            TestCasesRun.Add(testCase);

            return Task.FromResult(result);
        }
    }
}
