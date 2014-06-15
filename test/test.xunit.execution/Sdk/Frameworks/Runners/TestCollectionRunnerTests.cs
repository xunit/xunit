using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestCollectionRunnerTests
{
    [Fact]
    public static async void Messages()
    {
        var summary = new RunSummary { Total = 4, Failed = 2, Skipped = 1, Time = 21.12m };
        var messageBus = new SpyMessageBus();
        var testCase = Mocks.TestCase<ClassUnderTest>("Passing");
        var runner = TestableTestCollectionRunner.Create(messageBus, new[] { testCase }, summary);

        var result = await runner.RunAsync();

        Assert.Equal(result.Total, summary.Total);
        Assert.Equal(result.Failed, summary.Failed);
        Assert.Equal(result.Skipped, summary.Skipped);
        Assert.Equal(result.Time, summary.Time);
        Assert.False(runner.TokenSource.IsCancellationRequested);
        Assert.Collection(messageBus.Messages,
            msg =>
            {
                var starting = Assert.IsAssignableFrom<ITestCollectionStarting>(msg);
                Assert.Same(testCase.TestCollection, starting.TestCollection);
            },
            msg =>
            {
                var finished = Assert.IsAssignableFrom<ITestCollectionFinished>(msg);
                Assert.Same(testCase.TestCollection, finished.TestCollection);
                Assert.Equal(21.12m, finished.ExecutionTime);
                Assert.Equal(4, finished.TestsRun);
                Assert.Equal(2, finished.TestsFailed);
                Assert.Equal(1, finished.TestsSkipped);
            }
        );
    }

    [Fact]
    public static async void Cancellation_TestCollectionStarting_CallsOuterMethodsOnly()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestCollectionStarting));
        var runner = TestableTestCollectionRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
        Assert.True(runner.OnTestCollectionStarting_Called);
        Assert.False(runner.OnTestCollectionStarted_Called);
        Assert.False(runner.OnTestCollectionFinishing_Called);
        Assert.True(runner.OnTestCollectionFinished_Called);
    }

    [Fact]
    public static async void Cancellation_TestCollectionFinished_CallsOuterAndInnerMethods()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestCollectionFinished));
        var runner = TestableTestCollectionRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
        Assert.True(runner.OnTestCollectionStarting_Called);
        Assert.True(runner.OnTestCollectionStarted_Called);
        Assert.True(runner.OnTestCollectionFinishing_Called);
        Assert.True(runner.OnTestCollectionFinished_Called);
    }

    [Fact]
    public static async void TestsAreGroupedByCollection()
    {
        var passing1 = Mocks.TestCase<ClassUnderTest>("Passing");
        var other1 = Mocks.TestCase<ClassUnderTest>("Other");
        var passing2 = Mocks.TestCase<ClassUnderTest2>("Passing");
        var other2 = Mocks.TestCase<ClassUnderTest2>("Other");
        var runner = TestableTestCollectionRunner.Create(testCases: new[] { passing1, passing2, other2, other1 });

        await runner.RunAsync();

        Assert.Collection(runner.ClassesRun,
            tuple =>
            {
                Assert.Equal("TestCollectionRunnerTests+ClassUnderTest", tuple.Item1.Name);
                Assert.Collection(tuple.Item2,
                    testCase => Assert.Same(passing1, testCase),
                    testCase => Assert.Same(other1, testCase)
                );
            },
            tuple =>
            {
                Assert.Equal("TestCollectionRunnerTests+ClassUnderTest2", tuple.Item1.Name);
                Assert.Collection(tuple.Item2,
                    testCase => Assert.Same(passing2, testCase),
                    testCase => Assert.Same(other2, testCase)
                );
            }
        );
    }

    [Fact]
    public static async void SignalingCancellationStopsRunningClasses()
    {
        var passing1 = Mocks.TestCase<ClassUnderTest>("Passing");
        var passing2 = Mocks.TestCase<ClassUnderTest2>("Passing");
        var runner = TestableTestCollectionRunner.Create(testCases: new[] { passing1, passing2 }, cancelInRunTestClassAsync: true);

        await runner.RunAsync();

        var tuple = Assert.Single(runner.ClassesRun);
        Assert.Equal("TestCollectionRunnerTests+ClassUnderTest", tuple.Item1.Name);
    }

    class ClassUnderTest
    {
        [Fact]
        public void Passing() { }

        [Fact]
        public void Other() { }
    }

    class ClassUnderTest2 : ClassUnderTest { }

    class TestableTestCollectionRunner : TestCollectionRunner<ITestCase>
    {
        readonly bool cancelInRunTestClassAsync;
        readonly RunSummary result;

        public readonly List<Tuple<IReflectionTypeInfo, IEnumerable<ITestCase>>> ClassesRun = new List<Tuple<IReflectionTypeInfo, IEnumerable<ITestCase>>>();
        public bool OnTestCollectionFinished_Called;
        public bool OnTestCollectionFinishing_Called;
        public bool OnTestCollectionStarted_Called;
        public bool OnTestCollectionStarting_Called;
        public readonly CancellationTokenSource TokenSource;

        TestableTestCollectionRunner(ITestCollection testCollection,
                                     IEnumerable<ITestCase> testCases,
                                     IMessageBus messageBus,
                                     ITestCaseOrderer testCaseOrderer,
                                     CancellationTokenSource cancellationTokenSource,
                                     RunSummary result,
                                     bool cancelInRunTestClassAsync)
            : base(testCollection, testCases, messageBus, testCaseOrderer, cancellationTokenSource)
        {
            TokenSource = cancellationTokenSource;

            this.result = result;
            this.cancelInRunTestClassAsync = cancelInRunTestClassAsync;
        }

        public static TestableTestCollectionRunner Create(IMessageBus messageBus = null,
                                                          ITestCase[] testCases = null,
                                                          RunSummary result = null,
                                                          bool cancelInRunTestClassAsync = false)
        {
            if (testCases == null)
                testCases = new[] { Mocks.TestCase<ClassUnderTest>("Passing") };

            return new TestableTestCollectionRunner(
                testCases.First().TestCollection,
                testCases,
                messageBus ?? new SpyMessageBus(),
                new MockTestCaseOrderer(),
                new CancellationTokenSource(),
                result ?? new RunSummary(),
                cancelInRunTestClassAsync
            );
        }

        protected override void OnTestCollectionFinished()
        {
            OnTestCollectionFinished_Called = true;
        }

        protected override void OnTestCollectionFinishing()
        {
            OnTestCollectionFinishing_Called = true;
        }

        protected override void OnTestCollectionStarted()
        {
            OnTestCollectionStarted_Called = true;
        }

        protected override void OnTestCollectionStarting()
        {
            OnTestCollectionStarting_Called = true;
        }

        protected override Task<RunSummary> RunTestClassAsync(IReflectionTypeInfo testClass, IEnumerable<ITestCase> testCases)
        {
            if (cancelInRunTestClassAsync)
                CancellationTokenSource.Cancel();

            ClassesRun.Add(Tuple.Create(testClass, testCases));
            return Task.FromResult(result);
        }
    }
}
