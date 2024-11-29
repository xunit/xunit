using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public sealed class ExecutionErrorTestCaseTests : IDisposable
{
	readonly ExceptionAggregator aggregator = new();
	readonly SpyMessageBus messageBus = new();
	readonly CancellationTokenSource tokenSource = new();

	public void Dispose()
	{
		messageBus.Dispose();
		tokenSource.Dispose();
	}

	[Fact]
	public async ValueTask Messages_WithoutAggregatedError()
	{
		var testCase = ExecutionErrorTestCase("This is my error message");

		var result = await XunitRunnerHelper.RunXunitTestCase(testCase, messageBus, tokenSource, aggregator, ExplicitOption.Off, []);

		Assert.Equal(1, result.Total);
		Assert.Equal(0m, result.Time);
		Assert.Collection(
			messageBus.Messages,
			msg => Assert.IsAssignableFrom<ITestStarting>(msg),
			msg =>
			{
				var failed = Assert.IsAssignableFrom<ITestFailed>(msg);
				Assert.Equal(0m, failed.ExecutionTime);
				Assert.Empty(failed.Output);
				var exceptionType = Assert.Single(failed.ExceptionTypes);
				Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionType);
				var type = Assert.Single(failed.Messages);
				Assert.Equal("This is my error message", type);
			},
			msg =>
			{
				var testFinished = Assert.IsAssignableFrom<ITestFinished>(msg);
				Assert.Equal(0m, testFinished.ExecutionTime);
				Assert.Empty(testFinished.Output);
			}
		);
	}

	[Fact]
	public async ValueTask Messages_WithAggregatedError()
	{
		var testCase = ExecutionErrorTestCase("This is my error message");
		aggregator.Add(new DivideByZeroException());

		var result = await XunitRunnerHelper.RunXunitTestCase(testCase, messageBus, tokenSource, aggregator, ExplicitOption.Off, []);

		Assert.Equal(1, result.Total);
		Assert.Equal(0m, result.Time);
		Assert.Collection(
			messageBus.Messages,
			msg => Assert.IsAssignableFrom<ITestStarting>(msg),
			msg =>
			{
				var failed = Assert.IsAssignableFrom<ITestFailed>(msg);
				Assert.Equal(0m, failed.ExecutionTime);
				Assert.Empty(failed.Output);
				Assert.Equal([-1, 0, 0], failed.ExceptionParentIndices);
				Assert.Equal(new[] { typeof(AggregateException).SafeName(), typeof(DivideByZeroException).SafeName(), typeof(TestPipelineException).SafeName() }, failed.ExceptionTypes);
				Assert.Equal(["Attempted to divide by zero.", "This is my error message"], failed.Messages.Skip(1));  // We skip the AggregateException message because it changes between NetFx and NetCore
			},
			msg =>
			{
				var testFinished = Assert.IsAssignableFrom<ITestFinished>(msg);
				Assert.Equal(0m, testFinished.ExecutionTime);
				Assert.Empty(testFinished.Output);
			}
		);
	}

	[Theory]
	[InlineData(typeof(ITestStarting))]
	[InlineData(typeof(ITestFailed))]
	[InlineData(typeof(ITestFinished))]
	public async ValueTask Cancellation_TriggersCancellationTokenSource(Type messageTypeToCancelOn)
	{
		var testCase = ExecutionErrorTestCase("This is my error message");
		var messageBus = new SpyMessageBus(msg => !messageTypeToCancelOn.IsAssignableFrom(msg.GetType()));

		await XunitRunnerHelper.RunXunitTestCase(testCase, messageBus, tokenSource, aggregator, ExplicitOption.Off, []);

		Assert.True(tokenSource.IsCancellationRequested);
	}

	public static ExecutionErrorTestCase ExecutionErrorTestCase(string message)
	{
		var testMethod = TestData.XunitTestMethod<ExecutionErrorTestCaseTests>(methodName: nameof(Messages_WithoutAggregatedError));

		return new(
			testMethod,
			"test-case-display-name",
			"test-case-unique-id",
			message
		);
	}
}
