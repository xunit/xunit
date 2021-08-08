using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestCaseRunnerTests
{
	[Fact]
	public static async void Messages()
	{
		var summary = new RunSummary { Total = 4, Failed = 2, Skipped = 1, Time = 21.12m };
		var messageBus = new SpyMessageBus();
		var runner = TestableTestCaseRunner.Create(messageBus, result: summary);

		var result = await runner.RunAsync();

		Assert.Same(result, summary);
		Assert.False(runner.TokenSource.IsCancellationRequested);
		Assert.Collection(
			messageBus.Messages,
			msg => Assert.IsAssignableFrom<_TestCaseStarting>(msg),
			msg =>
			{
				var testCaseFinished = Assert.IsAssignableFrom<_TestCaseFinished>(msg);
				Assert.Equal(21.12m, testCaseFinished.ExecutionTime);
				Assert.Equal(4, testCaseFinished.TestsRun);
				Assert.Equal(2, testCaseFinished.TestsFailed);
				Assert.Equal(1, testCaseFinished.TestsSkipped);
			}
		);
	}

	[Fact]
	public static async void FailureInQueueOfTestCaseStarting_DoesNotQueueTestCaseFinished_DoesNotRunTests()
	{
		var messages = new List<_MessageSinkMessage>();
		var messageBus = Substitute.For<IMessageBus>();
		messageBus
			.QueueMessage(null!)
			.ReturnsForAnyArgs(callInfo =>
			{
				var msg = callInfo.Arg<_MessageSinkMessage>();
				messages.Add(msg);

				if (msg is _TestCaseStarting)
					throw new InvalidOperationException();

				return true;
			});
		var runner = TestableTestCaseRunner.Create(messageBus);

		await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync());

		var starting = Assert.Single(messages);
		Assert.IsAssignableFrom<_TestCaseStarting>(starting);
		Assert.False(runner.RunTestAsync_Called);
	}

	[Fact]
	public static async void RunTestAsync_AggregatorIncludesPassedInExceptions()
	{
		var messageBus = new SpyMessageBus();
		var ex = new DivideByZeroException();
		var runner = TestableTestCaseRunner.Create(messageBus, aggregatorSeedException: ex);

		await runner.RunAsync();

		Assert.Same(ex, runner.RunTestAsync_AggregatorResult);
		Assert.Empty(messageBus.Messages.OfType<_TestCaseCleanupFailure>());
	}

	[Fact]
	public static async void FailureInAfterTestCaseStarting_GivesErroredAggregatorToTestRunner_NoCleanupFailureMessage()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableTestCaseRunner.Create(messageBus);
		var ex = new DivideByZeroException();
		runner.AfterTestCaseStarting_Callback = aggregator => aggregator.Add(ex);

		await runner.RunAsync();

		Assert.Same(ex, runner.RunTestAsync_AggregatorResult);
		Assert.Empty(messageBus.Messages.OfType<_TestCaseCleanupFailure>());
	}

	[Fact]
	public static async void FailureInBeforeTestCaseFinished_ReportsCleanupFailure_DoesNotIncludeExceptionsFromAfterTestCaseStarting()
	{
		var messageBus = new SpyMessageBus();
		var testCase = Mocks.TestCase<TestAssemblyRunnerTests.RunAsync>("Messages");
		var runner = TestableTestCaseRunner.Create(messageBus, testCase);
		var startingException = new DivideByZeroException();
		var finishedException = new InvalidOperationException();
		runner.AfterTestCaseStarting_Callback = aggregator => aggregator.Add(startingException);
		runner.BeforeTestCaseFinished_Callback = aggregator => aggregator.Add(finishedException);

		await runner.RunAsync();

		var cleanupFailure = Assert.Single(messageBus.Messages.OfType<_TestCaseCleanupFailure>());
		Assert.Equal(typeof(InvalidOperationException).FullName, cleanupFailure.ExceptionTypes.Single());
	}

	[Fact]
	public static async void Cancellation_TestCaseStarting_DoesNotCallExtensibilityMethods()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestCaseStarting));
		var runner = TestableTestCaseRunner.Create(messageBus);

		await runner.RunAsync();

		Assert.False(runner.AfterTestCaseStarting_Called);
		Assert.False(runner.BeforeTestCaseFinished_Called);
	}

	[Fact]
	public static async void Cancellation_TestCaseFinished_CallsExtensibilityMethods()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestCaseFinished));
		var runner = TestableTestCaseRunner.Create(messageBus);

		await runner.RunAsync();

		Assert.True(runner.AfterTestCaseStarting_Called);
		Assert.True(runner.BeforeTestCaseFinished_Called);
	}

	[Fact]
	public static async void Cancellation_TestClassCleanupFailure_SetsCancellationToken()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestCaseCleanupFailure));
		var runner = TestableTestCaseRunner.Create(messageBus);
		runner.BeforeTestCaseFinished_Callback = aggregator => aggregator.Add(new Exception());

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
	}

	[Theory(DisableDiscoveryEnumeration = true)]
	[InlineData(typeof(_TestCaseStarting))]
	[InlineData(typeof(_TestCaseFinished))]
	public static async void Cancellation_TriggersCancellationTokenSource(Type messageTypeToCancelOn)
	{
		var messageBus = new SpyMessageBus(msg => !(messageTypeToCancelOn.IsAssignableFrom(msg.GetType())));
		var runner = TestableTestCaseRunner.Create(messageBus);

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
	}

	[Fact]
	public static async void TestContextInspection()
	{
		var runner = TestableTestCaseRunner.Create();

		await runner.RunAsync();

		Assert.NotNull(runner.AfterTestCaseStarting_Context);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestCaseStarting_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestCaseStarting_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestCaseStarting_Context.TestClassStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestCaseStarting_Context.TestMethodStatus);
		Assert.Equal(TestEngineStatus.Initializing, runner.AfterTestCaseStarting_Context.TestCaseStatus);
		Assert.Null(runner.AfterTestCaseStarting_Context.TestStatus);
		Assert.Same(runner.TestCase, runner.AfterTestCaseStarting_Context.TestCase);

		Assert.NotNull(runner.RunTestAsync_Context);
		Assert.Equal(TestEngineStatus.Running, runner.RunTestAsync_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.RunTestAsync_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Running, runner.RunTestAsync_Context.TestClassStatus);
		Assert.Equal(TestEngineStatus.Running, runner.RunTestAsync_Context.TestMethodStatus);
		Assert.Equal(TestEngineStatus.Running, runner.RunTestAsync_Context.TestCaseStatus);
		Assert.Null(runner.RunTestAsync_Context.TestStatus);
		Assert.Same(runner.TestCase, runner.RunTestAsync_Context.TestCase);

		Assert.NotNull(runner.BeforeTestCaseStarting_Context);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestCaseStarting_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestCaseStarting_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestCaseStarting_Context.TestClassStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestCaseStarting_Context.TestMethodStatus);
		Assert.Equal(TestEngineStatus.CleaningUp, runner.BeforeTestCaseStarting_Context.TestCaseStatus);
		Assert.Null(runner.BeforeTestCaseStarting_Context.TestStatus);
		Assert.Same(runner.TestCase, runner.BeforeTestCaseStarting_Context.TestCase);
	}

	class TestableTestCaseRunner : TestCaseRunner<_ITestCase>
	{
		readonly RunSummary result;

		public Action<ExceptionAggregator> AfterTestCaseStarting_Callback = _ => { };
		public bool AfterTestCaseStarting_Called;
		public TestContext? AfterTestCaseStarting_Context;
		public Action<ExceptionAggregator> BeforeTestCaseFinished_Callback = _ => { };
		public bool BeforeTestCaseFinished_Called;
		public TestContext? BeforeTestCaseStarting_Context;
		public Exception? RunTestAsync_AggregatorResult;
		public bool RunTestAsync_Called;
		public TestContext? RunTestAsync_Context;
		public CancellationTokenSource TokenSource;

		TestableTestCaseRunner(
			_ITestCase testCase,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource tokenSource,
			RunSummary result)
				: base(testCase, messageBus, aggregator, tokenSource)
		{
			this.result = result;

			TokenSource = tokenSource;
		}

		public new _ITestCase? TestCase => base.TestCase;

		public static TestableTestCaseRunner Create(
			IMessageBus? messageBus = null,
			_ITestCase? testCase = null,
			RunSummary? result = null,
			Exception? aggregatorSeedException = null)
		{
			var aggregator = new ExceptionAggregator();
			if (aggregatorSeedException != null)
				aggregator.Add(aggregatorSeedException);

			return new TestableTestCaseRunner(
				testCase ?? Mocks.TestCase<object>("ToString"),
				messageBus ?? new SpyMessageBus(),
				aggregator,
				new CancellationTokenSource(),
				result ?? new RunSummary()
			);
		}

		protected override Task AfterTestCaseStartingAsync()
		{
			AfterTestCaseStarting_Called = true;
			AfterTestCaseStarting_Context = TestContext.Current;
			AfterTestCaseStarting_Callback(Aggregator);
			return Task.CompletedTask;
		}

		protected override Task BeforeTestCaseFinishedAsync()
		{
			BeforeTestCaseFinished_Called = true;
			BeforeTestCaseStarting_Context = TestContext.Current;
			BeforeTestCaseFinished_Callback(Aggregator);
			return Task.CompletedTask;
		}

		protected override Task<RunSummary> RunTestAsync()
		{
			RunTestAsync_AggregatorResult = Aggregator.ToException();
			RunTestAsync_Called = true;
			RunTestAsync_Context = TestContext.Current;
			return Task.FromResult(result);
		}
	}
}
