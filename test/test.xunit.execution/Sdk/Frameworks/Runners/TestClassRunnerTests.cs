using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestClassRunnerTests
{
    [Fact]
    public static async void Messages()
    {
        var summary = new RunSummary { Total = 4, Failed = 2, Skipped = 1, Time = 21.12m };
        var messageBus = new SpyMessageBus();
        var testCase = Mocks.TestCase<ClassUnderTest>("Passing");
        var runner = TestableTestClassRunner.Create(messageBus, new[] { testCase }, result: summary);

        var result = await runner.RunAsync();

        Assert.Equal(result.Total, summary.Total);
        Assert.Equal(result.Failed, summary.Failed);
        Assert.Equal(result.Skipped, summary.Skipped);
        Assert.Equal(result.Time, summary.Time);
        Assert.False(runner.TokenSource.IsCancellationRequested);
        Assert.Collection(messageBus.Messages,
            msg =>
            {
                var starting = Assert.IsAssignableFrom<ITestClassStarting>(msg);
                Assert.Same(testCase.TestCollection, starting.TestCollection);
                Assert.Equal("TestClassRunnerTests+ClassUnderTest", starting.ClassName);
            },
            msg =>
            {
                var finished = Assert.IsAssignableFrom<ITestClassFinished>(msg);
                Assert.Same(testCase.TestCollection, finished.TestCollection);
                Assert.Equal("TestClassRunnerTests+ClassUnderTest", finished.ClassName);
                Assert.Equal(21.12m, finished.ExecutionTime);
                Assert.Equal(4, finished.TestsRun);
                Assert.Equal(2, finished.TestsFailed);
                Assert.Equal(1, finished.TestsSkipped);
            }
        );
    }

    [Fact]
    public static async void Cancellation_TestClassStarting_CallsOuterMethodsOnly()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestClassStarting));
        var runner = TestableTestClassRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
        Assert.True(runner.OnTestClassStarting_Called);
        Assert.False(runner.OnTestClassStarted_Called);
        Assert.False(runner.OnTestClassFinishing_Called);
        Assert.True(runner.OnTestClassFinished_Called);
    }

    [Fact]
    public static async void Cancellation_TestClassFinished_CallsOuterAndInnerMethods()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestClassFinished));
        var runner = TestableTestClassRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
        Assert.True(runner.OnTestClassStarting_Called);
        Assert.True(runner.OnTestClassStarted_Called);
        Assert.True(runner.OnTestClassFinishing_Called);
        Assert.True(runner.OnTestClassFinished_Called);
    }

    [Fact]
    public static async void TestsAreGroupedByMethod()
    {
        var passing1 = Mocks.TestCase<ClassUnderTest>("Passing");
        var passing2 = Mocks.TestCase<ClassUnderTest>("Passing");
        var other1 = Mocks.TestCase<ClassUnderTest>("Other");
        var other2 = Mocks.TestCase<ClassUnderTest>("Other");
        var runner = TestableTestClassRunner.Create(testCases: new[] { passing1, other1, other2, passing2 });

        await runner.RunAsync();

        Assert.Collection(runner.MethodsRun,
            tuple =>
            {
                Assert.Equal("Passing", tuple.Item1.Name);
                Assert.Collection(tuple.Item2,
                    testCase => Assert.Same(passing1, testCase),
                    testCase => Assert.Same(passing2, testCase)
                );
            },
            tuple =>
            {
                Assert.Equal("Other", tuple.Item1.Name);
                Assert.Collection(tuple.Item2,
                    testCase => Assert.Same(other1, testCase),
                    testCase => Assert.Same(other2, testCase)
                );
            }
        );
    }

    [Fact]
    public static async void SignalingCancellationStopsRunningMethods()
    {
        var passing = Mocks.TestCase<ClassUnderTest>("Passing");
        var other = Mocks.TestCase<ClassUnderTest>("Other");
        var runner = TestableTestClassRunner.Create(testCases: new[] { passing, other }, cancelInRunTestMethodAsync: true);

        await runner.RunAsync();

        var tuple = Assert.Single(runner.MethodsRun);
        Assert.Equal("Passing", tuple.Item1.Name);
    }

    [Fact]
    public static async void TestsOrdererIsUsedToDetermineRunOrder()
    {
        var passing1 = Mocks.TestCase<ClassUnderTest>("Passing");
        var passing2 = Mocks.TestCase<ClassUnderTest>("Passing");
        var other1 = Mocks.TestCase<ClassUnderTest>("Other");
        var other2 = Mocks.TestCase<ClassUnderTest>("Other");
        var runner = TestableTestClassRunner.Create(testCases: new[] { passing1, other1, passing2, other2 }, orderer: new MockTestCaseOrderer(reverse: true));

        await runner.RunAsync();

        Assert.Collection(runner.MethodsRun,
            tuple =>
            {
                Assert.Equal("Other", tuple.Item1.Name);
                Assert.Collection(tuple.Item2,
                    testCase => Assert.Same(other2, testCase),
                    testCase => Assert.Same(other1, testCase)
                );
            },
            tuple =>
            {
                Assert.Equal("Passing", tuple.Item1.Name);
                Assert.Collection(tuple.Item2,
                    testCase => Assert.Same(passing2, testCase),
                    testCase => Assert.Same(passing1, testCase)
                );
            }
        );
    }

    [Fact]
    public static async void TestClassMustHaveParameterlessConstructor()
    {
        var test = Mocks.TestCase<ClassWithConstructor>("Passing");
        var runner = TestableTestClassRunner.Create(testCases: new[] { test });

        await runner.RunAsync();

        var ex = runner.Aggregator.ToException();
        var tcex = Assert.IsType<TestClassException>(ex);
        Assert.Equal("A test class must have a parameterless constructor.", tcex.Message);
        Assert.NotEmpty(runner.MethodsRun);  // Error message passed into the method runner to become test failure
    }

    [Fact]
    public static async void ConstructorWithMissingArguments()
    {
        var test = Mocks.TestCase<ClassWithConstructor>("Passing");
        var constructor = typeof(ClassWithConstructor).GetConstructors().Single();
        var args = new object[] { "Hello, world!" };
        var runner = TestableTestClassRunner.Create(testCases: new[] { test }, constructor: constructor, availableArguments: args);

        await runner.RunAsync();

        var ex = runner.Aggregator.ToException();
        var tcex = Assert.IsType<TestClassException>(ex);
        Assert.Equal("The following constructor parameters did not have matching arguments: Int32 x, Decimal z", tcex.Message);
        Assert.NotEmpty(runner.MethodsRun);  // Error message passed into the method runner to become test failure
    }

    [Fact]
    public static async void ConstructorWithMatchingArguments()
    {
        var test = Mocks.TestCase<ClassWithConstructor>("Passing");
        var constructor = typeof(ClassWithConstructor).GetConstructors().Single();
        var args = new object[] { "Hello, world!", 21.12m, 42, DateTime.Now };
        var runner = TestableTestClassRunner.Create(testCases: new[] { test }, constructor: constructor, availableArguments: args);

        await runner.RunAsync();

        var tuple = Assert.Single(runner.MethodsRun);
        Assert.Collection(tuple.Item3,
            arg => Assert.Equal(42, arg),
            arg => Assert.Equal("Hello, world!", arg),
            arg => Assert.Equal(21.12m, arg)
        );
        Assert.False(runner.Aggregator.HasExceptions);
    }

    class ClassUnderTest
    {
        [Fact]
        public void Passing() { }

        [Fact]
        public void Other() { }
    }

    class ClassWithConstructor
    {
        public ClassWithConstructor(int x, string y, decimal z) { }

        [Fact]
        public void Passing() { }
    }
    
    class TestableTestClassRunner : TestClassRunner<ITestCase>
    {
        readonly object[] availableArguments;
        readonly bool cancelInRunTestMethodAsync;
        readonly ConstructorInfo constructor;
        readonly RunSummary result;

        public readonly new ExceptionAggregator Aggregator;
        public List<Tuple<IReflectionMethodInfo, IEnumerable<ITestCase>, object[]>> MethodsRun = new List<Tuple<IReflectionMethodInfo, IEnumerable<ITestCase>, object[]>>();
        public bool OnTestClassFinished_Called;
        public bool OnTestClassFinishing_Called;
        public bool OnTestClassStarted_Called;
        public bool OnTestClassStarting_Called;
        public readonly CancellationTokenSource TokenSource;

        TestableTestClassRunner(ITestCollection testCollection,
                                IReflectionTypeInfo testClass,
                                IEnumerable<ITestCase> testCases,
                                IMessageBus messageBus,
                                ITestCaseOrderer testCaseOrderer,
                                ExceptionAggregator aggregator,
                                CancellationTokenSource cancellationTokenSource,
                                ConstructorInfo constructor,
                                object[] availableArguments,
                                RunSummary result,
                                bool cancelInRunTestMethodAsync)
            : base(testCollection, testClass, testCases, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            Aggregator = aggregator;
            TokenSource = cancellationTokenSource;

            this.constructor = constructor;
            this.availableArguments = availableArguments;
            this.result = result;
            this.cancelInRunTestMethodAsync = cancelInRunTestMethodAsync;
        }

        public static TestableTestClassRunner Create(IMessageBus messageBus = null,
                                                     ITestCase[] testCases = null,
                                                     ITestCaseOrderer orderer = null,
                                                     ConstructorInfo constructor = null,
                                                     object[] availableArguments = null,
                                                     RunSummary result = null,
                                                     bool cancelInRunTestMethodAsync = false)
        {
            if (testCases == null)
                testCases = new[] { Mocks.TestCase<ClassUnderTest>("Passing") };
            if (availableArguments == null)
                availableArguments = new object[0];

            var firstTest = testCases.First();

            return new TestableTestClassRunner(
                firstTest.TestCollection,
                (IReflectionTypeInfo)firstTest.Class,
                testCases,
                messageBus ?? new SpyMessageBus(),
                orderer ?? new MockTestCaseOrderer(),
                new ExceptionAggregator(),
                new CancellationTokenSource(),
                constructor,
                availableArguments,
                result ?? new RunSummary(),
                cancelInRunTestMethodAsync
            );
        }

        protected override void OnTestClassFinished()
        {
            OnTestClassFinished_Called = true;
        }

        protected override void OnTestClassFinishing()
        {
            OnTestClassFinishing_Called = true;
        }

        protected override void OnTestClassStarted()
        {
            OnTestClassStarted_Called = true;
        }

        protected override void OnTestClassStarting()
        {
            OnTestClassStarting_Called = true;
        }

        protected override Task<RunSummary> RunTestMethodAsync(IReflectionMethodInfo testMethod, IEnumerable<ITestCase> testCases, object[] constructorArguments)
        {
            if (cancelInRunTestMethodAsync)
                CancellationTokenSource.Cancel();

            MethodsRun.Add(Tuple.Create(testMethod, testCases, constructorArguments));
            return Task.FromResult(result);
        }

        protected override ConstructorInfo SelectTestClassConstructor()
        {
            return constructor ?? base.SelectTestClassConstructor();
        }

        protected override bool TryGetConstructorArgument(ConstructorInfo constructor, int index, ParameterInfo parameter, out object argumentValue)
        {
            argumentValue = availableArguments.FirstOrDefault(arg => parameter.ParameterType.IsAssignableFrom(arg.GetType()));
            if (argumentValue != null)
                return true;

            return base.TryGetConstructorArgument(constructor, index, parameter, out argumentValue);
        }
    }
}
