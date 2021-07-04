using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

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
		Assert.Collection(
			messageBus.Messages,
			msg =>
			{
				var starting = Assert.IsAssignableFrom<_TestMethodStarting>(msg);
				Assert.Equal("Passing", starting.TestMethod);
			},
			msg =>
			{
				var finished = Assert.IsAssignableFrom<_TestMethodFinished>(msg);
				Assert.Equal(21.12m, finished.ExecutionTime);
				Assert.Equal(2, finished.TestsFailed);
				Assert.Equal(4, finished.TestsRun);
				Assert.Equal(1, finished.TestsSkipped);
			}
		);
	}

	[Fact]
	public static async void FailureInQueueOfTestMethodStarting_DoesNotQueueTestMethodFinished_DoesNotRunTestCases()
	{
		var messages = new List<_MessageSinkMessage>();
		var messageBus = Substitute.For<IMessageBus>();
		messageBus
			.QueueMessage(null!)
			.ReturnsForAnyArgs(callInfo =>
			{
				var msg = callInfo.Arg<_MessageSinkMessage>();
				messages.Add(msg);

				if (msg is _TestMethodStarting)
					throw new InvalidOperationException();

				return true;
			});
		var runner = TestableTestMethodRunner.Create(messageBus);

		await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync());

		var starting = Assert.Single(messages);
		Assert.IsAssignableFrom<_TestMethodStarting>(starting);
		Assert.Empty(runner.TestCasesRun);
	}

	[Fact]
	public static async void RunTestCaseAsync_AggregatorIncludesPassedInExceptions()
	{
		var messageBus = new SpyMessageBus();
		var ex = new DivideByZeroException();
		var runner = TestableTestMethodRunner.Create(messageBus, aggregatorSeedException: ex);

		await runner.RunAsync();

		Assert.Same(ex, runner.RunTestCaseAsync_AggregatorResult);
		Assert.Empty(messageBus.Messages.OfType<_TestMethodCleanupFailure>());
	}

	[Fact]
	public static async void FailureInAfterTestMethodStarting_GivesErroredAggregatorToTestCaseRunner_NoCleanupFailureMessage()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableTestMethodRunner.Create(messageBus);
		var ex = new DivideByZeroException();
		runner.AfterTestMethodStarting_Callback = aggregator => aggregator.Add(ex);

		await runner.RunAsync();

		Assert.Same(ex, runner.RunTestCaseAsync_AggregatorResult);
		Assert.Empty(messageBus.Messages.OfType<_TestMethodCleanupFailure>());
	}

	[Fact]
	public static async void FailureInBeforeTestMethodFinished_ReportsCleanupFailure_DoesNotIncludeExceptionsFromAfterTestMethodStarting()
	{
		var messageBus = new SpyMessageBus();
		var testCases = new[] { Mocks.TestCase<TestAssemblyRunnerTests.RunAsync>("Messages") };
		var runner = TestableTestMethodRunner.Create(messageBus, testCases);
		var startingException = new DivideByZeroException();
		var finishedException = new InvalidOperationException();
		runner.AfterTestMethodStarting_Callback = aggregator => aggregator.Add(startingException);
		runner.BeforeTestMethodFinished_Callback = aggregator => aggregator.Add(finishedException);

		await runner.RunAsync();

		var cleanupFailure = Assert.Single(messageBus.Messages.OfType<_TestMethodCleanupFailure>());
		Assert.Equal(typeof(InvalidOperationException).FullName, cleanupFailure.ExceptionTypes.Single());
	}

	[Fact]
	public static async void Cancellation_TestMethodStarting_DoesNotCallExtensibilityMethods()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestMethodStarting));
		var runner = TestableTestMethodRunner.Create(messageBus);

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
		Assert.False(runner.AfterTestMethodStarting_Called);
		Assert.False(runner.BeforeTestMethodFinished_Called);
	}

	[Fact]
	public static async void Cancellation_TestMethodFinished_CallsExtensibilityMethods()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestMethodFinished));
		var runner = TestableTestMethodRunner.Create(messageBus);

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
		Assert.True(runner.AfterTestMethodStarting_Called);
		Assert.True(runner.BeforeTestMethodFinished_Called);
	}

	[Fact]
	public static async void Cancellation_TestMethodCleanupFailure_SetsCancellationToken()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestMethodCleanupFailure));
		var runner = TestableTestMethodRunner.Create(messageBus);
		runner.BeforeTestMethodFinished_Callback = aggregator => aggregator.Add(new Exception());

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
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

	class TestableTestMethodRunner : TestMethodRunner<_ITestCase>
	{
		readonly bool cancelInRunTestCaseAsync;
		readonly RunSummary result;

		public bool AfterTestMethodStarting_Called;
		public Action<ExceptionAggregator> AfterTestMethodStarting_Callback = _ => { };
		public bool BeforeTestMethodFinished_Called;
		public Action<ExceptionAggregator> BeforeTestMethodFinished_Callback = _ => { };
		public Exception? RunTestCaseAsync_AggregatorResult;
		public readonly CancellationTokenSource TokenSource;

		public List<_ITestCase> TestCasesRun = new List<_ITestCase>();

		TestableTestMethodRunner(
			_ITestMethod testMethod,
			_IReflectionTypeInfo @class,
			_IReflectionMethodInfo method,
			IReadOnlyCollection<_ITestCase> testCases,
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

		public static TestableTestMethodRunner Create(
			IMessageBus? messageBus = null,
			_ITestCase[]? testCases = null,
			RunSummary? result = null,
			Exception? aggregatorSeedException = null,
			bool cancelInRunTestCaseAsync = false)
		{
			if (testCases == null)
				testCases = new[] { Mocks.TestCase<ClassUnderTest>("Passing") };

			var firstTestCase = testCases.First();

			var aggregator = new ExceptionAggregator();
			if (aggregatorSeedException != null)
				aggregator.Add(aggregatorSeedException);

			return new TestableTestMethodRunner(
				firstTestCase.TestMethod,
				(_IReflectionTypeInfo)firstTestCase.TestMethod.TestClass.Class,
				(_IReflectionMethodInfo)firstTestCase.TestMethod.Method,
				testCases,
				messageBus ?? new SpyMessageBus(),
				aggregator,
				new CancellationTokenSource(),
				result ?? new RunSummary(),
				cancelInRunTestCaseAsync
			);
		}

		protected override void AfterTestMethodStarting()
		{
			AfterTestMethodStarting_Called = true;
			AfterTestMethodStarting_Callback(Aggregator);
		}

		protected override void BeforeTestMethodFinished()
		{
			BeforeTestMethodFinished_Called = true;
			BeforeTestMethodFinished_Callback(Aggregator);
		}

		protected override Task<RunSummary> RunTestCaseAsync(_ITestCase testCase)
		{
			if (cancelInRunTestCaseAsync)
				CancellationTokenSource.Cancel();

			RunTestCaseAsync_AggregatorResult = Aggregator.ToException();
			TestCasesRun.Add(testCase);

			return Task.FromResult(result);
		}
	}
}
