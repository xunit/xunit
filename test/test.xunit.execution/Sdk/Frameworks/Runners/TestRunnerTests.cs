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
                Assert.Equal("Display Name", testStarting.Test.DisplayName);
            },
            msg => { },  // Pass/fail/skip, will be tested elsewhere
            msg =>
            {
                var testFinished = Assert.IsAssignableFrom<ITestFinished>(msg);
                Assert.Same(runner.TestCase.TestMethod.TestClass.TestCollection, testFinished.TestCollection);
                Assert.Same(runner.TestCase, testFinished.TestCase);
                Assert.Equal("Display Name", testFinished.Test.DisplayName);
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
        Assert.Equal("Display Name", passed.Test.DisplayName);
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
        Assert.Equal("Display Name", failed.Test.DisplayName);
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
        Assert.Equal("Display Name", failed.Test.DisplayName);
        Assert.Equal(0m, failed.ExecutionTime);
        Assert.Empty(failed.Output);
        Assert.Equal("Please don't run me", failed.Reason);
    }

    [Fact]
    public static async void Output()
    {
        var messageBus = new SpyMessageBus();
        var runner = TestableTestRunner.Create(messageBus, output: "This is my text output");

        await runner.RunAsync();

        var passed = messageBus.Messages.OfType<ITestPassed>().Single();
        Assert.Equal("This is my text output", passed.Output);
    }

    [Fact]
    public static async void FailureInQueueOfTestStarting_DoesNotQueueTestFinished_DoesNotInvokeTest()
    {
        var messages = new List<IMessageSinkMessage>();
        var messageBus = Substitute.For<IMessageBus>();
        messageBus.QueueMessage(null)
                  .ReturnsForAnyArgs(callInfo =>
                  {
                      var msg = callInfo.Arg<IMessageSinkMessage>();
                      messages.Add(msg);

                      if (msg is ITestStarting)
                          throw new InvalidOperationException();

                      return true;
                  });
        var runner = TestableTestRunner.Create(messageBus);

        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync());

        var starting = Assert.Single(messages);
        Assert.IsAssignableFrom<ITestStarting>(starting);
        Assert.False(runner.InvokeTestAsync_Called);
    }

    [Fact]
    public static async void WithPreSeededException_ReturnsTestFailed_NoCleanupFailureMessage()
    {
        var messageBus = new SpyMessageBus();
        var ex = new DivideByZeroException();
        var runner = TestableTestRunner.Create(messageBus, aggregatorSeedException: ex);

        await runner.RunAsync();

        var failed = Assert.Single(messageBus.Messages.OfType<ITestFailed>());
        Assert.Equal(typeof(DivideByZeroException).FullName, failed.ExceptionTypes.Single());
        Assert.Empty(messageBus.Messages.OfType<ITestCleanupFailure>());
    }

    [Fact]
    public static async void FailureInAfterTestStarting_ReturnsTestFailed_NoCleanupFailureMessage()
    {
        var messageBus = new SpyMessageBus();
        var runner = TestableTestRunner.Create(messageBus);
        var ex = new DivideByZeroException();
        runner.AfterTestStarting_Callback = aggregator => aggregator.Add(ex);

        await runner.RunAsync();

        var failed = Assert.Single(messageBus.Messages.OfType<ITestFailed>());
        Assert.Equal(typeof(DivideByZeroException).FullName, failed.ExceptionTypes.Single());
        Assert.Empty(messageBus.Messages.OfType<ITestCleanupFailure>());
    }

    [Fact]
    public static async void FailureInBeforeTestFinished_ReportsCleanupFailure_DoesNotIncludeExceptionsFromAfterTestStarting()
    {
        var messageBus = new SpyMessageBus();
        var testCase = Mocks.TestCase<TestAssemblyRunnerTests.RunAsync>("Messages");
        var runner = TestableTestRunner.Create(messageBus, testCase);
        var startingException = new DivideByZeroException();
        var finishedException = new InvalidOperationException();
        runner.AfterTestStarting_Callback = aggregator => aggregator.Add(startingException);
        runner.BeforeTestFinished_Callback = aggregator => aggregator.Add(finishedException);

        await runner.RunAsync();

        var cleanupFailure = Assert.Single(messageBus.Messages.OfType<ITestCleanupFailure>());
        Assert.Same(testCase, cleanupFailure.TestCase);
        Assert.Equal(new[] { testCase }, cleanupFailure.TestCases);
        Assert.Equal(typeof(InvalidOperationException).FullName, cleanupFailure.ExceptionTypes.Single());
    }

    [Fact]
    public static async void Cancellation_TestStarting_DoesNotCallExtensibilityMethods()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestStarting));
        var runner = TestableTestRunner.Create(messageBus);

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
        Assert.False(runner.AfterTestStarting_Called);
        Assert.False(runner.BeforeTestFinished_Called);
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
        Assert.True(runner.AfterTestStarting_Called);
        Assert.True(runner.BeforeTestFinished_Called);
    }

    [Fact]
    public static async void Cancellation_TestCleanupFailure_SetsCancellationToken()
    {
        var messageBus = new SpyMessageBus(msg => !(msg is ITestCleanupFailure));
        var runner = TestableTestRunner.Create(messageBus);
        runner.BeforeTestFinished_Callback = aggregator => aggregator.Add(new Exception());

        await runner.RunAsync();

        Assert.True(runner.TokenSource.IsCancellationRequested);
    }

    class TestableTestRunner : TestRunner<ITestCase>
    {
        readonly Action lambda;
        readonly string output;
        readonly decimal runTime;

        public bool InvokeTestAsync_Called;
        public Action<ExceptionAggregator> AfterTestStarting_Callback = _ => { };
        public bool AfterTestStarting_Called;
        public Action<ExceptionAggregator> BeforeTestFinished_Callback = _ => { };
        public bool BeforeTestFinished_Called;
        public readonly new ITestCase TestCase;
        public CancellationTokenSource TokenSource;

        TestableTestRunner(ITest test,
                           IMessageBus messageBus,
                           Type testClass,
                           object[] constructorArguments,
                           MethodInfo testMethod,
                           object[] testMethodArguments,
                           string skipReason,
                           ExceptionAggregator aggregator,
                           CancellationTokenSource cancellationTokenSource,
                           decimal runTime,
                           string output,
                           Action lambda)
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, skipReason, aggregator, cancellationTokenSource)
        {
            TestCase = test.TestCase;
            TokenSource = cancellationTokenSource;

            this.runTime = runTime;
            this.output = output;
            this.lambda = lambda;
        }

        public static TestableTestRunner Create(IMessageBus messageBus,
                                                ITestCase testCase = null,
                                                string displayName = null,
                                                string skipReason = null,
                                                decimal runTime = 0m,
                                                string output = "",
                                                Exception aggregatorSeedException = null,
                                                Action lambda = null)
        {
            var aggregator = new ExceptionAggregator();
            if (aggregatorSeedException != null)
                aggregator.Add(aggregatorSeedException);
            if (testCase == null)
                testCase = Mocks.TestCase<object>("ToString");
            var test = Mocks.Test(testCase, displayName);

            return new TestableTestRunner(
                test,
                messageBus,
                typeof(object),
                new object[0],
                typeof(object).GetMethod("ToString"),
                new object[0],
                skipReason,
                aggregator,
                new CancellationTokenSource(),
                runTime,
                output,
                lambda);
        }

        protected override void AfterTestStarting()
        {
            AfterTestStarting_Called = true;
            AfterTestStarting_Callback(Aggregator);
        }

        protected override void BeforeTestFinished()
        {
            BeforeTestFinished_Called = true;
            BeforeTestFinished_Callback(Aggregator);
        }

        protected override Task<Tuple<decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator)
        {
            if (lambda != null)
                aggregator.Run(lambda);

            InvokeTestAsync_Called = true;

            return Task.FromResult(Tuple.Create(runTime, output));
        }
    }
}
