using System;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class ExecutionErrorTestCaseRunnerTests : IDisposable
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
        var testCase = Mocks.ExecutionErrorTestCase("This is my error message");
        var runner = new ExecutionErrorTestCaseRunner(testCase, messageBus, aggregator, tokenSource);

        var result = await runner.RunAsync();

        Assert.Equal(1, result.Total);
        Assert.Equal(0m, result.Time);
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
            },
            msg =>
            {
                var failed = Assert.IsAssignableFrom<ITestFailed>(msg);
                Assert.Same(testCase.TestMethod.TestClass.TestCollection, failed.TestCollection);
                Assert.Same(testCase, failed.TestCase);
                Assert.Equal(0m, failed.ExecutionTime);
                Assert.Empty(failed.Output);
                Assert.Collection(failed.ExceptionTypes, type => Assert.Equal("System.InvalidOperationException", type));
                Assert.Collection(failed.Messages, type => Assert.Equal("This is my error message", type));
            },
            msg =>
            {
                var testFinished = Assert.IsAssignableFrom<ITestFinished>(msg);
                Assert.Same(testCase.TestMethod.TestClass.TestCollection, testFinished.TestCollection);
                Assert.Same(testCase, testFinished.TestCase);
                Assert.Equal(0m, testFinished.ExecutionTime);
                Assert.Empty(testFinished.Output);
            },
            msg =>
            {
                var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(msg);
                Assert.Same(testCase.TestMethod.TestClass.TestCollection, testCaseFinished.TestCollection);
                Assert.Same(testCase, testCaseFinished.TestCase);
                Assert.Equal(0m, testCaseFinished.ExecutionTime);
                Assert.Equal(1, testCaseFinished.TestsRun);
                Assert.Equal(1, testCaseFinished.TestsFailed);
                Assert.Equal(0, testCaseFinished.TestsSkipped);
            }
        );
    }

    [Theory]
    [InlineData(typeof(ITestStarting))]
    [InlineData(typeof(ITestFailed))]
    [InlineData(typeof(ITestFinished))]
    public async void Cancellation_TriggersCancellationTokenSource(Type messageTypeToCancelOn)
    {
        var testCase = Mocks.ExecutionErrorTestCase("This is my error message");
        var messageBus = new SpyMessageBus(msg => !(messageTypeToCancelOn.IsAssignableFrom(msg.GetType())));
        var runner = new ExecutionErrorTestCaseRunner(testCase, messageBus, aggregator, tokenSource);

        await runner.RunAsync();

        Assert.True(tokenSource.IsCancellationRequested);
    }
}
