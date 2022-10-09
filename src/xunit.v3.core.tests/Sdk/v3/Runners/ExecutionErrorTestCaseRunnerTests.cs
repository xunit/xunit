using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class ExecutionErrorTestCaseRunnerTests : IDisposable
{
	readonly ExceptionAggregator aggregator = new();
	readonly SpyMessageBus messageBus = new();
	readonly CancellationTokenSource tokenSource = new();

	public void Dispose()
	{
		messageBus?.Dispose();
		tokenSource?.Dispose();
	}

	[Fact]
	public async ValueTask Messages()
	{
		var testCase = ExecutionErrorTestCase("This is my error message");

		var result = await ExecutionErrorTestCaseRunner.Instance.RunAsync(testCase, messageBus, aggregator, tokenSource);

		Assert.Equal(1, result.Total);
		Assert.Equal(0m, result.Time);
		Assert.Collection(
			messageBus.Messages,
			msg => Assert.IsAssignableFrom<_TestCaseStarting>(msg),
			msg => Assert.IsAssignableFrom<_TestStarting>(msg),
			msg =>
			{
				var failed = Assert.IsAssignableFrom<_TestFailed>(msg);
				Assert.Equal(0m, failed.ExecutionTime);
				Assert.Empty(failed.Output);
				var exceptionType = Assert.Single(failed.ExceptionTypes);
				Assert.Equal("System.InvalidOperationException", exceptionType);
				var type = Assert.Single(failed.Messages);
				Assert.Equal("This is my error message", type);
			},
			msg =>
			{
				var testFinished = Assert.IsAssignableFrom<_TestFinished>(msg);
				Assert.Equal(0m, testFinished.ExecutionTime);
				Assert.Empty(testFinished.Output);
			},
			msg =>
			{
				var testCaseFinished = Assert.IsAssignableFrom<_TestCaseFinished>(msg);
				Assert.Equal(0m, testCaseFinished.ExecutionTime);
				Assert.Equal(1, testCaseFinished.TestsFailed);
				Assert.Equal(0, testCaseFinished.TestsNotRun);
				Assert.Equal(0, testCaseFinished.TestsSkipped);
				Assert.Equal(1, testCaseFinished.TestsTotal);
			}
		);
	}

	[Theory]
	[InlineData(typeof(_TestStarting))]
	[InlineData(typeof(_TestFailed))]
	[InlineData(typeof(_TestFinished))]
	public async ValueTask Cancellation_TriggersCancellationTokenSource(Type messageTypeToCancelOn)
	{
		var testCase = ExecutionErrorTestCase("This is my error message");
		var messageBus = new SpyMessageBus(msg => !messageTypeToCancelOn.IsAssignableFrom(msg.GetType()));

		await ExecutionErrorTestCaseRunner.Instance.RunAsync(testCase, messageBus, aggregator, tokenSource);

		Assert.True(tokenSource.IsCancellationRequested);
	}

	public static ExecutionErrorTestCase ExecutionErrorTestCase(string message)
	{
		var testMethod = Mocks.TestMethod();

		return new(
			testMethod,
			"test-case-display-name",
			"test-case-unique-id",
			message
		);
	}
}
