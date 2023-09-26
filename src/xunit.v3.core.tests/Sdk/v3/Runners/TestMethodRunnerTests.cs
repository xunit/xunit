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
	public static async ValueTask Messages()
	{
		var summary = new RunSummary { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12m };
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
				Assert.Equal(3, finished.TestsNotRun);
				Assert.Equal(1, finished.TestsSkipped);
				Assert.Equal(9, finished.TestsTotal);
			}
		);
	}

	[Fact]
	public static async ValueTask FailureInQueueOfTestMethodStarting_DoesNotQueueTestMethodFinished_DoesNotRunTestCases()
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

		var ex = await Record.ExceptionAsync(() => runner.RunAsync());

		Assert.IsType<InvalidOperationException>(ex);
		var starting = Assert.Single(messages);
		Assert.IsAssignableFrom<_TestMethodStarting>(starting);
		Assert.Empty(runner.TestCasesRun);
	}

	[Fact]
	public static async ValueTask RunTestCaseAsync_AggregatorIncludesPassedInExceptions()
	{
		var messageBus = new SpyMessageBus();
		var ex = new DivideByZeroException();
		var runner = TestableTestMethodRunner.Create(messageBus, aggregatorSeedException: ex);

		await runner.RunAsync();

		Assert.Same(ex, runner.RunTestCaseAsync_AggregatorResult);
		Assert.Empty(messageBus.Messages.OfType<_TestMethodCleanupFailure>());
	}

	[Fact]
	public static async ValueTask FailureInAfterTestMethodStarting_GivesErroredAggregatorToTestCaseRunner_NoCleanupFailureMessage()
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
	public static async ValueTask FailureInBeforeTestMethodFinished_ReportsCleanupFailure_DoesNotIncludeExceptionsFromAfterTestMethodStarting()
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
	public static async ValueTask Cancellation_TestMethodStarting_DoesNotCallExtensibilityMethods()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestMethodStarting));
		var runner = TestableTestMethodRunner.Create(messageBus);

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
		Assert.False(runner.AfterTestMethodStarting_Called);
		Assert.False(runner.BeforeTestMethodFinished_Called);
	}

	[Fact]
	public static async ValueTask Cancellation_TestMethodFinished_CallsExtensibilityMethods()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestMethodFinished));
		var runner = TestableTestMethodRunner.Create(messageBus);

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
		Assert.True(runner.AfterTestMethodStarting_Called);
		Assert.True(runner.BeforeTestMethodFinished_Called);
	}

	[Fact]
	public static async ValueTask Cancellation_TestMethodCleanupFailure_SetsCancellationToken()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestMethodCleanupFailure));
		var runner = TestableTestMethodRunner.Create(messageBus);
		runner.BeforeTestMethodFinished_Callback = aggregator => aggregator.Add(new Exception());

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
	}

	[Fact]
	public static async ValueTask SignalingCancellationStopsRunningMethods()
	{
		var passing = Mocks.TestCase<ClassUnderTest>("Passing");
		var other = Mocks.TestCase<ClassUnderTest>("Other");
		var runner = TestableTestMethodRunner.Create(testCases: new[] { passing, other }, cancelInRunTestCaseAsync: true);

		await runner.RunAsync();

		var testCase = Assert.Single(runner.TestCasesRun);
		Assert.Same(passing, testCase);
	}

	[Fact]
	public static async ValueTask TestContextInspection()
	{
		var runner = TestableTestMethodRunner.Create();

		await runner.RunAsync();

		Assert.NotNull(runner.AfterTestMethodStarting_Context);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestMethodStarting_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestMethodStarting_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestMethodStarting_Context.TestClassStatus);
		Assert.Equal(TestEngineStatus.Initializing, runner.AfterTestMethodStarting_Context.TestMethodStatus);
		Assert.Equal(TestPipelineStage.TestMethodExecution, runner.AfterTestMethodStarting_Context.PipelineStage);
		Assert.Null(runner.AfterTestMethodStarting_Context.TestCaseStatus);
		Assert.Null(runner.AfterTestMethodStarting_Context.TestStatus);
		Assert.Same(runner.TestMethod, runner.AfterTestMethodStarting_Context.TestMethod);

		Assert.NotNull(runner.RunTestCaseAsync_Context);
		Assert.Equal(TestEngineStatus.Running, runner.RunTestCaseAsync_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.RunTestCaseAsync_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Running, runner.RunTestCaseAsync_Context.TestClassStatus);
		Assert.Equal(TestEngineStatus.Running, runner.RunTestCaseAsync_Context.TestMethodStatus);
		Assert.Null(runner.RunTestCaseAsync_Context.TestCaseStatus);
		Assert.Null(runner.RunTestCaseAsync_Context.TestStatus);
		Assert.Same(runner.TestMethod, runner.RunTestCaseAsync_Context.TestMethod);

		Assert.NotNull(runner.BeforeTestMethodFinished_Context);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestMethodFinished_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestMethodFinished_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestMethodFinished_Context.TestClassStatus);
		Assert.Equal(TestEngineStatus.CleaningUp, runner.BeforeTestMethodFinished_Context.TestMethodStatus);
		Assert.Null(runner.BeforeTestMethodFinished_Context.TestCaseStatus);
		Assert.Null(runner.BeforeTestMethodFinished_Context.TestStatus);
		Assert.Same(runner.TestMethod, runner.BeforeTestMethodFinished_Context.TestMethod);
	}

	class ClassUnderTest
	{
		[Fact]
		public void Passing() { }

		[Fact]
		public void Other() { }
	}

	class TestableTestMethodRunner : TestMethodRunner<TestMethodRunnerContext<_ITestCase>, _ITestCase>
	{
		readonly bool cancelInRunTestCaseAsync;
		readonly _IReflectionTypeInfo @class;
		readonly IMessageBus messageBus;
		readonly _IReflectionMethodInfo method;
		readonly RunSummary result;
		readonly IReadOnlyCollection<_ITestCase> testCases;
		readonly _ITestClass testClass;

		public readonly ExceptionAggregator Aggregator;
		public bool AfterTestMethodStarting_Called;
		public TestContext? AfterTestMethodStarting_Context;
		public Action<ExceptionAggregator> AfterTestMethodStarting_Callback = _ => { };
		public bool BeforeTestMethodFinished_Called;
		public TestContext? BeforeTestMethodFinished_Context;
		public Action<ExceptionAggregator> BeforeTestMethodFinished_Callback = _ => { };
		public Exception? RunTestCaseAsync_AggregatorResult;
		public TestContext? RunTestCaseAsync_Context;
		public readonly _ITestMethod TestMethod;
		public readonly CancellationTokenSource TokenSource;

		public List<_ITestCase> TestCasesRun = new();

		TestableTestMethodRunner(
			_ITestClass testClass,
			_ITestMethod testMethod,
			_IReflectionTypeInfo @class,
			_IReflectionMethodInfo method,
			IReadOnlyCollection<_ITestCase> testCases,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource,
			RunSummary result,
			bool cancelInRunTestCaseAsync)
		{
			this.testClass = testClass;
			TestMethod = testMethod;
			this.@class = @class;
			this.method = method;
			this.testCases = testCases;
			this.messageBus = messageBus;
			Aggregator = aggregator;
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
			if (testCases is null)
				testCases = new[] { Mocks.TestCase<ClassUnderTest>("Passing") };

			var firstTestCase = testCases.First();

			var aggregator = new ExceptionAggregator();
			if (aggregatorSeedException is not null)
				aggregator.Add(aggregatorSeedException);

			return new TestableTestMethodRunner(
				firstTestCase.TestClass ?? throw new InvalidOperationException("testCase.TestClass must not be null"),
				firstTestCase.TestMethod ?? throw new InvalidOperationException("testCase.TestMethod must not be null"),
				firstTestCase.TestClass.Class as _IReflectionTypeInfo ?? throw new InvalidOperationException("testCase.TestClass.Class must be based on reflection"),
				firstTestCase.TestMethod.Method as _IReflectionMethodInfo ?? throw new InvalidOperationException("testCase.TestMethod.Method must be based on reflection"),
				testCases,
				messageBus ?? new SpyMessageBus(),
				aggregator,
				new CancellationTokenSource(),
				result ?? new RunSummary(),
				cancelInRunTestCaseAsync
			);
		}

		protected override ValueTask AfterTestMethodStarting(TestMethodRunnerContext<_ITestCase> ctxt)
		{
			AfterTestMethodStarting_Called = true;
			AfterTestMethodStarting_Context = TestContext.Current;
			AfterTestMethodStarting_Callback(Aggregator);

			return default;
		}

		protected override ValueTask BeforeTestMethodFinished(TestMethodRunnerContext<_ITestCase> ctxt)
		{
			BeforeTestMethodFinished_Called = true;
			BeforeTestMethodFinished_Context = TestContext.Current;
			BeforeTestMethodFinished_Callback(Aggregator);

			return default;
		}

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new TestMethodRunnerContext<_ITestCase>(testClass, TestMethod, @class, method, testCases, ExplicitOption.Off, messageBus, Aggregator, TokenSource);
			await ctxt.InitializeAsync();

			return await RunAsync(ctxt);
		}

		protected override ValueTask<RunSummary> RunTestCaseAsync(
			TestMethodRunnerContext<_ITestCase> ctxt,
			_ITestCase testCase)
		{
			if (cancelInRunTestCaseAsync)
				TokenSource.Cancel();

			RunTestCaseAsync_AggregatorResult = Aggregator.ToException();
			RunTestCaseAsync_Context = TestContext.Current;
			TestCasesRun.Add(testCase);

			return new(result);
		}
	}
}
