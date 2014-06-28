using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestRunnerTests
{
    [Fact]
    public static async void Messages()
    {
        var messageBus = new SpyMessageBus();
        var runner = TestableTestRunner.Create(messageBus, displayName: "Display Name", runTime: 21.12m);

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
        var runner = TestableTestRunner.Create(messageBus, displayName: "Display Name", runTime: 21.12m);

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
        var runner = TestableTestRunner.Create(messageBus, displayName: "Display Name", runTime: 21.12m, lambda: () => Assert.True(false));

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
        var runner = TestableTestRunner.Create(messageBus, displayName: "Display Name", skipReason: "Please don't run me", runTime: 21.12m, lambda: () => Assert.True(false));

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
    public static async void FailureInQueueOfTestStarting_DoesNotQueueTestFinished_DoesNotInvokeTest()
    {
        var messages = new List<IMessageSinkMessage>();
        var messageBus = Substitute.For<IMessageBus>();
        messageBus.QueueMessage(null)
                  .Returns(callInfo =>
                  {
                      var msg = callInfo.Arg<IMessageSinkMessage>();
                      messages.Add(msg);

                      if (msg is ITestStarting)
                          throw new InvalidOperationException();

                      return true;
                  });
        var runner = TestableTestRunner.Create(messageBus);

        await runner.RunAsync();

        var starting = Assert.Single(messages);
        Assert.IsAssignableFrom<ITestStarting>(starting);
        Assert.False(runner.InvokeTestAsync_Called);
    }

    [Fact]
    public static async void InvokeTestAsync_AggregatorIncludesPassedInExceptions()
    {
        var messageBus = new SpyMessageBus();
        var ex = new DivideByZeroException();
        var runner = TestableTestRunner.Create(messageBus, aggregatorSeedException: ex);

        await runner.RunAsync();

        Assert.Same(ex, runner.InvokeTestAsync_AggregatorResult);
    }

    [Fact]
    public static async void FailureInOnTestStarted_GivesErroredAggregatorToTestInvoker()
    {
        var messageBus = new SpyMessageBus();
        var runner = TestableTestRunner.Create(messageBus);
        var ex = new DivideByZeroException();
        runner.OnTestStarted_Callback = aggregator => aggregator.Add(ex);

        await runner.RunAsync();

        Assert.Same(ex, runner.InvokeTestAsync_AggregatorResult);
    }

    [Fact]
    public static async void Cancellation_TestStarting_DoesNotCallExtensibilityMethods()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestStarting));
        var runner = TestableTestRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
        Assert.False(runner.OnTestStarted_Called);
        Assert.False(runner.OnTestFinishing_Called);
    }

    [Theory]
    [InlineData(typeof(ITestPassed), true, null)]
    [InlineData(typeof(ITestFailed), false, null)]
    [InlineData(typeof(ITestSkipped), false, "Please skip me")]
    [InlineData(typeof(ITestFinished), true, null)]
    public static async void Cancellation_AllOthers_CallsExtensibilityMethods(Type messageTypeToCancelOn, bool shouldTestPass, string skipReason = null)
    {
        var messageBus = new SpyMessageBus(msg => !(messageTypeToCancelOn.IsAssignableFrom(msg.GetType())));
        var runner = TestableTestRunner.Create(messageBus, skipReason: skipReason, lambda: () => Assert.True(shouldTestPass));

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
        Assert.True(runner.OnTestStarted_Called);
        Assert.True(runner.OnTestFinishing_Called);
    }

    class TestableTestRunner : TestRunner<ITestCase>
    {
        readonly Action lambda;
        readonly decimal runTime;

        public Exception InvokeTestAsync_AggregatorResult;
        public bool InvokeTestAsync_Called;
        public Action<ExceptionAggregator> OnTestFinishing_Callback = _ => { };
        public bool OnTestFinishing_Called;
        public Action<ExceptionAggregator> OnTestStarted_Callback = _ => { };
        public bool OnTestStarted_Called;
        public readonly new ITestCase TestCase;
        public CancellationTokenSource TokenSource;

        TestableTestRunner(ITestCase testCase,
                           IMessageBus messageBus,
                           Type testClass,
                           object[] constructorArguments,
                           MethodInfo testMethod,
                           object[] testMethodArguments,
                           string displayName,
                           string skipReason,
                           ExceptionAggregator aggregator,
                           CancellationTokenSource cancellationTokenSource,
                           decimal runTime,
                           Action lambda)
            : base(testCase, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, displayName, skipReason, aggregator, cancellationTokenSource)
        {
            TestCase = testCase;
            TokenSource = cancellationTokenSource;

            this.runTime = runTime;
            this.lambda = lambda;
        }

        public static TestableTestRunner Create(IMessageBus messageBus,
                                                ITestCase testCase = null,
                                                string displayName = null,
                                                string skipReason = null,
                                                decimal runTime = 0m,
                                                Exception aggregatorSeedException = null,
                                                Action lambda = null)
        {
            var aggregator = new ExceptionAggregator();
            if (aggregatorSeedException != null)
                aggregator.Add(aggregatorSeedException);

            return new TestableTestRunner(
                testCase ?? Mocks.TestCase<Object>("ToString"),
                messageBus,
                typeof(Object),
                new object[0],
                typeof(Object).GetMethod("ToString"),
                new object[0],
                displayName,
                skipReason,
                aggregator,
                new CancellationTokenSource(),
                runTime,
                lambda);
        }

        protected override void OnTestFinishing()
        {
            OnTestFinishing_Called = true;
            OnTestFinishing_Callback(Aggregator);
        }

        protected override void OnTestStarted()
        {
            OnTestStarted_Called = true;
            OnTestStarted_Callback(Aggregator);
        }

        protected override Task<decimal> InvokeTestAsync()
        {
            if (lambda != null)
                Aggregator.Run(lambda);

            InvokeTestAsync_AggregatorResult = Aggregator.ToException();
            InvokeTestAsync_Called = true;

            return Task.FromResult(runTime);
        }
    }
}
