using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestCollectionRunnerTests
{
	[Fact]
	public static async void Messages()
	{
		var summary = new RunSummary { Total = 4, Failed = 2, Skipped = 1, Time = 21.12m };
		var messageBus = new SpyMessageBus();
		var testCase = Mocks.TestCase<ClassUnderTest>("Passing");
		var runner = TestableTestCollectionRunner.Create(messageBus, new[] { testCase }, summary);

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
				var starting = Assert.IsAssignableFrom<_TestCollectionStarting>(msg);
				Assert.Equal("assembly-id", starting.AssemblyUniqueID);
				Assert.Null(starting.TestCollectionClass);
				Assert.Equal("Mock test collection", starting.TestCollectionDisplayName);
				Assert.Equal("collection-id", starting.TestCollectionUniqueID);
			},
			msg =>
			{
				var finished = Assert.IsAssignableFrom<_TestCollectionFinished>(msg);
				Assert.Equal("assembly-id", finished.AssemblyUniqueID);
				Assert.Equal(21.12m, finished.ExecutionTime);
				Assert.Equal("collection-id", finished.TestCollectionUniqueID);
				Assert.Equal(2, finished.TestsFailed);
				Assert.Equal(4, finished.TestsRun);
				Assert.Equal(1, finished.TestsSkipped);
			}
		);
	}

	[Fact]
	public static async void FailureInQueueOfTestCollectionStarting_DoesNotQueueTestCollectionFinished_DoesNotRunTestClasses()
	{
		var messages = new List<_MessageSinkMessage>();
		var messageBus = Substitute.For<IMessageBus>();
		messageBus
			.QueueMessage(null!)
			.ReturnsForAnyArgs(callInfo =>
			{
				var msg = callInfo.Arg<_MessageSinkMessage>();
				messages.Add(msg);

				if (msg is _TestCollectionStarting)
					throw new InvalidOperationException();

				return true;
			});
		var runner = TestableTestCollectionRunner.Create(messageBus);

		var ex = await Record.ExceptionAsync(() => runner.RunAsync());

		Assert.IsType<InvalidOperationException>(ex);
		var starting = Assert.Single(messages);
		Assert.IsAssignableFrom<_TestCollectionStarting>(starting);
		Assert.Empty(runner.ClassesRun);
	}

	[Fact]
	public static async void RunTestClassAsync_AggregatorIncludesPassedInExceptions()
	{
		var messageBus = new SpyMessageBus();
		var ex = new DivideByZeroException();
		var runner = TestableTestCollectionRunner.Create(messageBus, aggregatorSeedException: ex);

		await runner.RunAsync();

		Assert.Same(ex, runner.RunTestClassAsync_AggregatorResult);
		Assert.Empty(messageBus.Messages.OfType<_TestCollectionCleanupFailure>());
	}

	[Fact]
	public static async void FailureInAfterTestCollectionStarting_GivesErroredAggregatorToTestClassRunner_NoCleanupFailureMessage()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableTestCollectionRunner.Create(messageBus);
		var ex = new DivideByZeroException();
		runner.AfterTestCollectionStarting_Callback = aggregator => aggregator.Add(ex);

		await runner.RunAsync();

		Assert.Same(ex, runner.RunTestClassAsync_AggregatorResult);
		Assert.Empty(messageBus.Messages.OfType<_TestCollectionCleanupFailure>());
	}

	[Fact]
	public static async void FailureInBeforeTestCollectionFinished_ReportsCleanupFailure_DoesNotIncludeExceptionsFromAfterTestCollectionStarting()
	{
		var messageBus = new SpyMessageBus();
		var testCases = new[] { Mocks.TestCase<TestAssemblyRunnerTests.RunAsync>("Messages") };
		var runner = TestableTestCollectionRunner.Create(messageBus, testCases);
		var startingException = new DivideByZeroException();
		var finishedException = new InvalidOperationException();
		runner.AfterTestCollectionStarting_Callback = aggregator => aggregator.Add(startingException);
		runner.BeforeTestCollectionFinished_Callback = aggregator => aggregator.Add(finishedException);

		await runner.RunAsync();

		var cleanupFailure = Assert.Single(messageBus.Messages.OfType<_TestCollectionCleanupFailure>());
		Assert.Equal(typeof(InvalidOperationException).FullName, cleanupFailure.ExceptionTypes.Single());
	}

	[Fact]
	public static async void Cancellation_TestCollectionStarting_DoesNotCallExtensibilityCallbacks()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestCollectionStarting));
		var runner = TestableTestCollectionRunner.Create(messageBus);

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
		Assert.False(runner.AfterTestCollectionStarting_Called);
		Assert.False(runner.BeforeTestCollectionFinished_Called);
	}

	[Fact]
	public static async void Cancellation_TestCollectionFinished_CallsExtensibilityCallbacks()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestCollectionFinished));
		var runner = TestableTestCollectionRunner.Create(messageBus);

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
		Assert.True(runner.AfterTestCollectionStarting_Called);
		Assert.True(runner.BeforeTestCollectionFinished_Called);
	}

	[Fact]
	public static async void Cancellation_TestCollectionCleanupFailure_SetsCancellationToken()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestCollectionCleanupFailure));
		var runner = TestableTestCollectionRunner.Create(messageBus);
		runner.BeforeTestCollectionFinished_Callback = aggregator => aggregator.Add(new Exception());

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
	}

	[Fact]
	public static async void TestsAreGroupedByCollection()
	{
		var passing1 = Mocks.TestCase<ClassUnderTest>("Passing");
		var other1 = Mocks.TestCase<ClassUnderTest>("Other");
		var passing2 = Mocks.TestCase<ClassUnderTest2>("Passing");
		var other2 = Mocks.TestCase<ClassUnderTest2>("Other");
		var runner = TestableTestCollectionRunner.Create(testCases: new[] { passing1, passing2, other2, other1 });

		await runner.RunAsync();

		Assert.Collection(
			runner.ClassesRun,
			tuple =>
			{
				Assert.Equal("TestCollectionRunnerTests+ClassUnderTest", tuple.Item1?.Name);
				Assert.Collection(tuple.Item2,
					testCase => Assert.Same(passing1, testCase),
					testCase => Assert.Same(other1, testCase)
				);
			},
			tuple =>
			{
				Assert.Equal("TestCollectionRunnerTests+ClassUnderTest2", tuple.Item1?.Name);
				Assert.Collection(tuple.Item2,
					testCase => Assert.Same(passing2, testCase),
					testCase => Assert.Same(other2, testCase)
				);
			}
		);
	}

	[Fact]
	public static async void SignalingCancellationStopsRunningClasses()
	{
		var passing1 = Mocks.TestCase<ClassUnderTest>("Passing");
		var passing2 = Mocks.TestCase<ClassUnderTest2>("Passing");
		var runner = TestableTestCollectionRunner.Create(testCases: new[] { passing1, passing2 }, cancelInRunTestClassAsync: true);

		await runner.RunAsync();

		var tuple = Assert.Single(runner.ClassesRun);
		Assert.Equal("TestCollectionRunnerTests+ClassUnderTest", tuple.Item1?.Name);
	}

	[Fact]
	public static async void TestContextInspection()
	{
		var runner = TestableTestCollectionRunner.Create();

		await runner.RunAsync();

		Assert.NotNull(runner.AfterTestCollectionStarting_Context);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestCollectionStarting_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Initializing, runner.AfterTestCollectionStarting_Context.TestCollectionStatus);
		Assert.Equal(TestPipelineStage.TestCollectionExecution, runner.AfterTestCollectionStarting_Context.PipelineStage);
		Assert.Null(runner.AfterTestCollectionStarting_Context.TestClassStatus);
		Assert.Null(runner.AfterTestCollectionStarting_Context.TestMethodStatus);
		Assert.Null(runner.AfterTestCollectionStarting_Context.TestCaseStatus);
		Assert.Null(runner.AfterTestCollectionStarting_Context.TestStatus);
		Assert.Same(runner.TestCollection, runner.AfterTestCollectionStarting_Context.TestCollection);

		Assert.NotNull(runner.RunTestClassAsync_Context);
		Assert.Equal(TestEngineStatus.Running, runner.RunTestClassAsync_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.RunTestClassAsync_Context.TestCollectionStatus);
		Assert.Null(runner.RunTestClassAsync_Context.TestClassStatus);
		Assert.Null(runner.RunTestClassAsync_Context.TestMethodStatus);
		Assert.Null(runner.RunTestClassAsync_Context.TestCaseStatus);
		Assert.Null(runner.RunTestClassAsync_Context.TestStatus);
		Assert.Same(runner.TestCollection, runner.RunTestClassAsync_Context.TestCollection);

		Assert.NotNull(runner.BeforeTestCollectionFinished_Context);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestCollectionFinished_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.CleaningUp, runner.BeforeTestCollectionFinished_Context.TestCollectionStatus);
		Assert.Null(runner.BeforeTestCollectionFinished_Context.TestClassStatus);
		Assert.Null(runner.BeforeTestCollectionFinished_Context.TestMethodStatus);
		Assert.Null(runner.BeforeTestCollectionFinished_Context.TestCaseStatus);
		Assert.Null(runner.BeforeTestCollectionFinished_Context.TestStatus);
		Assert.Same(runner.TestCollection, runner.BeforeTestCollectionFinished_Context.TestCollection);
	}

	class ClassUnderTest
	{
		[Fact]
		public void Passing() { }

		[Fact]
		public void Other() { }
	}

	class ClassUnderTest2 : ClassUnderTest { }

	class TestableTestCollectionRunner : TestCollectionRunner<TestCollectionRunnerContext<_ITestCase>, _ITestCase>
	{
		readonly ExceptionAggregator aggregator;
		readonly bool cancelInRunTestClassAsync;
		readonly IMessageBus messageBus;
		readonly RunSummary result;
		readonly ITestCaseOrderer testCaseOrderer;
		readonly IReadOnlyCollection<_ITestCase> testCases;

		public readonly List<Tuple<_IReflectionTypeInfo?, IReadOnlyCollection<_ITestCase>>> ClassesRun = new();
		public Action<ExceptionAggregator> AfterTestCollectionStarting_Callback = _ => { };
		public bool AfterTestCollectionStarting_Called;
		public TestContext? AfterTestCollectionStarting_Context;
		public Action<ExceptionAggregator> BeforeTestCollectionFinished_Callback = _ => { };
		public bool BeforeTestCollectionFinished_Called;
		public TestContext? BeforeTestCollectionFinished_Context;
		public Exception? RunTestClassAsync_AggregatorResult;
		public TestContext? RunTestClassAsync_Context;
		public readonly _ITestCollection TestCollection;
		public readonly CancellationTokenSource TokenSource;

		TestableTestCollectionRunner(
			_ITestCollection testCollection,
			IReadOnlyCollection<_ITestCase> testCases,
			IMessageBus messageBus,
			ITestCaseOrderer testCaseOrderer,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource,
			RunSummary result,
			bool cancelInRunTestClassAsync)
		{
			TestCollection = testCollection;
			this.testCases = testCases;
			this.messageBus = messageBus;
			this.testCaseOrderer = testCaseOrderer;
			this.aggregator = aggregator;
			TokenSource = cancellationTokenSource;
			this.result = result;
			this.cancelInRunTestClassAsync = cancelInRunTestClassAsync;
		}

		public static TestableTestCollectionRunner Create(
			IMessageBus? messageBus = null,
			_ITestCase[]? testCases = null,
			RunSummary? result = null,
			Exception? aggregatorSeedException = null,
			bool cancelInRunTestClassAsync = false)
		{
			if (testCases == null)
				testCases = new[] { Mocks.TestCase<ClassUnderTest>("Passing") };

			var aggregator = new ExceptionAggregator();
			if (aggregatorSeedException != null)
				aggregator.Add(aggregatorSeedException);

			return new TestableTestCollectionRunner(
				testCases.First().TestCollection,
				testCases,
				messageBus ?? new SpyMessageBus(),
				new MockTestCaseOrderer(),
				aggregator,
				new CancellationTokenSource(),
				result ?? new RunSummary(),
				cancelInRunTestClassAsync
			);
		}

		protected override ValueTask AfterTestCollectionStartingAsync(TestCollectionRunnerContext<_ITestCase> ctxt)
		{
			AfterTestCollectionStarting_Called = true;
			AfterTestCollectionStarting_Context = TestContext.Current;
			AfterTestCollectionStarting_Callback(ctxt.Aggregator);
			return default;
		}

		protected override ValueTask BeforeTestCollectionFinishedAsync(TestCollectionRunnerContext<_ITestCase> ctxt)
		{
			BeforeTestCollectionFinished_Called = true;
			BeforeTestCollectionFinished_Context = TestContext.Current;
			BeforeTestCollectionFinished_Callback(ctxt.Aggregator);
			return default;
		}

		public ValueTask<RunSummary> RunAsync() =>
			RunAsync(new(TestCollection, testCases, messageBus, testCaseOrderer, aggregator, TokenSource));

		protected override ValueTask<RunSummary> RunTestClassAsync(
			TestCollectionRunnerContext<_ITestCase> ctxt,
			_ITestClass? testClass,
			_IReflectionTypeInfo? @class,
			IReadOnlyCollection<_ITestCase> testCases)
		{
			if (cancelInRunTestClassAsync)
				ctxt.CancellationTokenSource.Cancel();

			RunTestClassAsync_AggregatorResult = ctxt.Aggregator.ToException();
			RunTestClassAsync_Context = TestContext.Current;
			ClassesRun.Add(Tuple.Create(@class, testCases));
			return new(result);
		}
	}
}
