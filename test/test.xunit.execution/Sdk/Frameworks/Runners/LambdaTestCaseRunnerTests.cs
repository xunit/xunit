using System;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class LambdaTestCaseRunnerTests : IDisposable
{
    readonly ExceptionAggregator aggregator = new ExceptionAggregator();
    readonly SpyMessageBus messageBus = new SpyMessageBus();
    readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

    public void Dispose()
    {
        if (messageBus != null) messageBus.Dispose();
        if (tokenSource != null) tokenSource.Dispose();
    }

    [Fact]
    public async void Messages()
    {
        var testCase = Mocks.LambdaTestCase(() => { });
        var runner = new LambdaTestCaseRunner(testCase, messageBus, aggregator, tokenSource);

        var result = await runner.RunAsync();

        Assert.Equal(1, result.Total);
        Assert.NotEqual(0m, result.Time);
        Assert.Collection(messageBus.Messages,
            msg =>
            {
                var testCaseStarting = Assert.IsAssignableFrom<ITestCaseStarting>(msg);
                Assert.Same(testCase.TestMethod.TestClass.TestCollection, testCaseStarting.TestCollection);
                Assert.Same(testCase, testCaseStarting.TestCase);
            },
            msg =>
            {
                var testStarting = Assert.IsAssignableFrom<ITestStarting>(msg);
                Assert.Same(testCase.TestMethod.TestClass.TestCollection, testStarting.TestCollection);
                Assert.Same(testCase, testStarting.TestCase);
                Assert.Equal("MockType.MockMethod", testStarting.Test.DisplayName);
            },
            msg => { },  // Pass/fail/skip, will be tested elsewhere
            msg =>
            {
                var testFinished = Assert.IsAssignableFrom<ITestFinished>(msg);
                Assert.Same(testCase.TestMethod.TestClass.TestCollection, testFinished.TestCollection);
                Assert.Same(testCase, testFinished.TestCase);
                Assert.Equal("MockType.MockMethod", testFinished.Test.DisplayName);
                Assert.NotEqual(0m, testFinished.ExecutionTime);
                Assert.Empty(testFinished.Output);
            },
            msg =>
            {
                var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(msg);
                Assert.Same(testCase.TestMethod.TestClass.TestCollection, testCaseFinished.TestCollection);
                Assert.Same(testCase, testCaseFinished.TestCase);
                Assert.NotEqual(0m, testCaseFinished.ExecutionTime);
                Assert.Equal(1, testCaseFinished.TestsRun);
            }
        );
    }

    [Fact]
    public async void Success()
    {
        var testCase = Mocks.LambdaTestCase(() => { });
        var runner = new LambdaTestCaseRunner(testCase, messageBus, aggregator, tokenSource);

        var result = await runner.RunAsync();

        // Direct run summary
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
        Assert.Equal(0, result.Skipped);
        Assert.NotEqual(0m, result.Time);
        // Pass message
        var passed = messageBus.Messages.OfType<ITestPassed>().Single();
        Assert.Same(testCase.TestMethod.TestClass.TestCollection, passed.TestCollection);
        Assert.Same(testCase, passed.TestCase);
        Assert.NotEqual(0m, passed.ExecutionTime);
        Assert.Empty(passed.Output);
        // Test case run summary
        var testCaseFinished = messageBus.Messages.OfType<ITestCaseFinished>().Single();
        Assert.Equal(passed.ExecutionTime, testCaseFinished.ExecutionTime);
        Assert.Equal(1, testCaseFinished.TestsRun);
        Assert.Equal(0, testCaseFinished.TestsFailed);
        Assert.Equal(0, testCaseFinished.TestsSkipped);
    }

    [Fact]
    public async void Failure()
    {
        var testCase = Mocks.LambdaTestCase(() => { throw new DivideByZeroException(); });
        var runner = new LambdaTestCaseRunner(testCase, messageBus, aggregator, tokenSource);

        var result = await runner.RunAsync();

        // Direct run summary
        Assert.Equal(1, result.Total);
        Assert.Equal(1, result.Failed);
        Assert.Equal(0, result.Skipped);
        Assert.NotEqual(0m, result.Time);
        // Fail message
        var failed = messageBus.Messages.OfType<ITestFailed>().Single();
        Assert.Same(testCase.TestMethod.TestClass.TestCollection, failed.TestCollection);
        Assert.Same(testCase, failed.TestCase);
        Assert.NotEqual(0m, failed.ExecutionTime);
        Assert.Empty(failed.Output);
        Assert.Collection(failed.ExceptionTypes, type => Assert.Equal("System.DivideByZeroException", type));
        // Test case run summary
        var testCaseFinished = messageBus.Messages.OfType<ITestCaseFinished>().Single();
        Assert.Equal(failed.ExecutionTime, testCaseFinished.ExecutionTime);
        Assert.Equal(1, testCaseFinished.TestsRun);
        Assert.Equal(1, testCaseFinished.TestsFailed);
        Assert.Equal(0, testCaseFinished.TestsSkipped);
    }

    [Theory]
    [InlineData(typeof(ITestStarting), true)]
    [InlineData(typeof(ITestPassed), true)]
    [InlineData(typeof(ITestFailed), false)]
    [InlineData(typeof(ITestFinished), true)]
    public async void Cancellation_TriggersCancellationTokenSource(Type messageTypeToCancelOn, bool shouldTestPass)
    {
        var testCase = Mocks.LambdaTestCase(() => Assert.True(shouldTestPass));
        var messageBus = new SpyMessageBus(msg => !(messageTypeToCancelOn.IsAssignableFrom(msg.GetType())));
        var runner = new LambdaTestCaseRunner(testCase, messageBus, aggregator, tokenSource);

        await runner.RunAsync();

        Assert.True(tokenSource.IsCancellationRequested);
    }
}
