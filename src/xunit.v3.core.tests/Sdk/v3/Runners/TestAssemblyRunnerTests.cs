using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class TestAssemblyRunnerTests
{
	public class RunAsync
	{
		[Fact]
		public static async ValueTask Messages()
		{
			var summary = new RunSummary { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12m };
			var messages = new List<_MessageSinkMessage>();
			var messageSink = SpyMessageSink.Create(messages: messages);
			var runner = TestableTestAssemblyRunner.Create(messageSink, summary);
			var thisAssembly = Assembly.GetExecutingAssembly();

			var result = await runner.RunAsync();

			Assert.Equal(2, result.Failed);
			Assert.Equal(3, result.NotRun);
			Assert.Equal(1, result.Skipped);
			Assert.Equal(9, result.Total);
			Assert.NotEqual(21.12m, result.Time);  // Uses clock time, not result time
			Assert.Collection(
				messages,
				msg =>
				{
					var starting = Assert.IsAssignableFrom<_TestAssemblyStarting>(msg);
#if NETFRAMEWORK
					Assert.Equal(thisAssembly.GetLocalCodeBase(), starting.AssemblyPath);
					Assert.NotNull(runner.TestAssembly);
					Assert.Equal(runner.TestAssembly.ConfigFileName, starting.ConfigFilePath);
					Assert.Equal(".NETFramework,Version=v4.7.2", starting.TargetFramework);
#else
					Assert.Equal(".NETCoreApp,Version=v6.0", starting.TargetFramework);
#endif
					Assert.InRange(starting.StartTime, DateTime.Now.AddMinutes(-15), DateTime.Now);
					Assert.Equal("The test framework environment", starting.TestEnvironment);
					Assert.Equal("The test framework display name", starting.TestFrameworkDisplayName);
					Assert.Equal("assembly-id", starting.AssemblyUniqueID);
				},
				msg =>
				{
					var finished = Assert.IsAssignableFrom<_TestAssemblyFinished>(msg);
					Assert.Equal("assembly-id", finished.AssemblyUniqueID);
					Assert.Equal(result.Time, finished.ExecutionTime);
					Assert.Equal(2, finished.TestsFailed);
					Assert.Equal(3, finished.TestsNotRun);
					Assert.Equal(1, finished.TestsSkipped);
					Assert.Equal(9, finished.TestsTotal);
				}
			);
		}

		[Fact]
		public static async ValueTask FailureInQueueOfTestAssemblyStarting_DoesNotQueueTestAssemblyFinished_DoesNotRunTestCollections()
		{
			var messages = new List<_MessageSinkMessage>();
			var messageSink = Substitute.For<_IMessageSink>();
			messageSink
				.OnMessage(null!)
				.ReturnsForAnyArgs(callInfo =>
				{
					var msg = callInfo.Arg<_MessageSinkMessage>();
					messages.Add(msg);

					if (msg is _TestAssemblyStarting)
						throw new InvalidOperationException();

					return true;
				});
			var runner = TestableTestAssemblyRunner.Create(messageSink);

			var ex = await Record.ExceptionAsync(() => runner.RunAsync());

			Assert.IsType<InvalidOperationException>(ex);
			var starting = Assert.Single(messages);
			Assert.IsAssignableFrom<_TestAssemblyStarting>(starting);
			Assert.Empty(runner.CollectionsRun);
		}

		[Fact]
		public static async ValueTask FailureInAfterTestAssemblyStarting_GivesErroredAggregatorToTestCollectionRunner_NoCleanupFailureMessage()
		{
			var messages = new List<_MessageSinkMessage>();
			var messageSink = SpyMessageSink.Create(messages: messages);
			var runner = TestableTestAssemblyRunner.Create(messageSink);
			var ex = new DivideByZeroException();
			runner.AfterTestAssemblyStarting_Callback = aggregator => aggregator.Add(ex);

			await runner.RunAsync();

			Assert.Same(ex, runner.RunTestCollectionAsync_AggregatorResult);
			Assert.Empty(messages.OfType<_TestAssemblyCleanupFailure>());
		}

		[Fact]
		public static async ValueTask FailureInBeforeTestAssemblyFinished_ReportsCleanupFailure_DoesNotIncludeExceptionsFromAfterTestAssemblyStarting()
		{
			var thisAssembly = Assembly.GetExecutingAssembly();
			var messages = new List<_MessageSinkMessage>();
			var messageSink = SpyMessageSink.Create(messages: messages);
			var testCases = new[] { TestCaseForTestCollection() };
			var runner = TestableTestAssemblyRunner.Create(messageSink, testCases: testCases);
			var startingException = new DivideByZeroException();
			var finishedException = new InvalidOperationException();
			runner.AfterTestAssemblyStarting_Callback = aggregator => aggregator.Add(startingException);
			runner.BeforeTestAssemblyFinished_Callback = aggregator => aggregator.Add(finishedException);

			await runner.RunAsync();

			var assemblyStarting = Assert.Single(messages.OfType<_TestAssemblyStarting>());
			var cleanupFailure = Assert.Single(messages.OfType<_TestAssemblyCleanupFailure>());
#if NETFRAMEWORK
			Assert.Equal(thisAssembly.GetLocalCodeBase(), assemblyStarting.AssemblyPath);
			Assert.NotNull(runner.TestAssembly);
			Assert.Equal(runner.TestAssembly.ConfigFileName, assemblyStarting.ConfigFilePath);
#endif
			Assert.Equal(typeof(InvalidOperationException).FullName, cleanupFailure.ExceptionTypes.Single());
		}

		[Fact]
		public static async ValueTask Cancellation_TestAssemblyStarting_DoesNotCallExtensibilityCallbacks()
		{
			var messageSink = SpyMessageSink.Create(msg => !(msg is _TestAssemblyStarting));
			var runner = TestableTestAssemblyRunner.Create(messageSink);

			await runner.RunAsync();

			Assert.False(runner.AfterTestAssemblyStarting_Called);
			Assert.False(runner.BeforeTestAssemblyFinished_Called);
		}

		[Fact]
		public static async ValueTask Cancellation_TestAssemblyFinished_CallsCallExtensibilityCallbacks()
		{
			var messageSink = SpyMessageSink.Create(msg => !(msg is _TestAssemblyFinished));
			var runner = TestableTestAssemblyRunner.Create(messageSink);

			await runner.RunAsync();

			Assert.True(runner.AfterTestAssemblyStarting_Called);
			Assert.True(runner.BeforeTestAssemblyFinished_Called);
		}

		[Fact]
		public static async ValueTask TestsAreGroupedByCollection()
		{
			var collection1 = Mocks.TestCollection(displayName: "1", uniqueID: "collection-1");
			var testCase1a = TestCaseForTestCollection(collection1);
			var testCase1b = TestCaseForTestCollection(collection1);
			var collection2 = Mocks.TestCollection(displayName: "2", uniqueID: "collection-2");
			var testCase2a = TestCaseForTestCollection(collection2);
			var testCase2b = TestCaseForTestCollection(collection2);
			var runner = TestableTestAssemblyRunner.Create(testCases: new[] { testCase1a, testCase2a, testCase2b, testCase1b });

			await runner.RunAsync();

			Assert.Collection(
				runner.CollectionsRun.OrderBy(c => c.Item1.DisplayName),
				tuple =>
				{
					Assert.Same(collection1, tuple.Item1);
					Assert.Collection(tuple.Item2,
						testCase => Assert.Same(testCase1a, testCase),
						testCase => Assert.Same(testCase1b, testCase)
					);
				},
				tuple =>
				{
					Assert.Same(collection2, tuple.Item1);
					Assert.Collection(tuple.Item2,
						testCase => Assert.Same(testCase2a, testCase),
						testCase => Assert.Same(testCase2b, testCase)
					);
				}
			);
		}

		[Fact]
		public static async ValueTask SignalingCancellationStopsRunningCollections()
		{
			var collection1 = Mocks.TestCollection();
			var testCase1 = TestCaseForTestCollection(collection1);
			var collection2 = Mocks.TestCollection();
			var testCase2 = TestCaseForTestCollection(collection2);
			var runner = TestableTestAssemblyRunner.Create(testCases: new[] { testCase1, testCase2 }, cancelInRunTestCollectionAsync: true);

			await runner.RunAsync();

			Assert.Single(runner.CollectionsRun);
		}

		[Fact]
		public static async ValueTask TestContextInspection()
		{
			var runner = TestableTestAssemblyRunner.Create();

			await runner.RunAsync();

			Assert.NotNull(runner.AfterTestAssemblyStarting_Context);
			Assert.Equal(TestEngineStatus.Initializing, runner.AfterTestAssemblyStarting_Context.TestAssemblyStatus);
			Assert.Equal(TestPipelineStage.TestAssemblyExecution, runner.AfterTestAssemblyStarting_Context.PipelineStage);
			Assert.Null(runner.AfterTestAssemblyStarting_Context.TestCollectionStatus);
			Assert.Null(runner.AfterTestAssemblyStarting_Context.TestClassStatus);
			Assert.Null(runner.AfterTestAssemblyStarting_Context.TestMethodStatus);
			Assert.Null(runner.AfterTestAssemblyStarting_Context.TestCaseStatus);
			Assert.Null(runner.AfterTestAssemblyStarting_Context.TestStatus);
			Assert.Same(runner.TestAssembly, runner.AfterTestAssemblyStarting_Context.TestAssembly);

			Assert.NotNull(runner.RunTestCollectionAsync_Context);
			Assert.Equal(TestEngineStatus.Running, runner.RunTestCollectionAsync_Context.TestAssemblyStatus);
			Assert.Null(runner.RunTestCollectionAsync_Context.TestCollectionStatus);
			Assert.Null(runner.RunTestCollectionAsync_Context.TestClassStatus);
			Assert.Null(runner.RunTestCollectionAsync_Context.TestMethodStatus);
			Assert.Null(runner.RunTestCollectionAsync_Context.TestCaseStatus);
			Assert.Null(runner.RunTestCollectionAsync_Context.TestStatus);
			Assert.Same(runner.TestAssembly, runner.RunTestCollectionAsync_Context.TestAssembly);

			Assert.NotNull(runner.BeforeTestAssemblyFinished_Context);
			Assert.Equal(TestEngineStatus.CleaningUp, runner.BeforeTestAssemblyFinished_Context.TestAssemblyStatus);
			Assert.Null(runner.BeforeTestAssemblyFinished_Context.TestCollectionStatus);
			Assert.Null(runner.BeforeTestAssemblyFinished_Context.TestClassStatus);
			Assert.Null(runner.BeforeTestAssemblyFinished_Context.TestMethodStatus);
			Assert.Null(runner.BeforeTestAssemblyFinished_Context.TestCaseStatus);
			Assert.Null(runner.BeforeTestAssemblyFinished_Context.TestStatus);
			Assert.Same(runner.TestAssembly, runner.BeforeTestAssemblyFinished_Context.TestAssembly);
		}
	}

	public static class TestCaseOrderer
	{
		[Fact]
		public static async ValueTask DefaultTestCaseOrderer()
		{
			var runner = TestableTestAssemblyRunner.Create();

			await runner.RunAsync();

			Assert.IsType<DefaultTestCaseOrderer>(runner.DefaultTestCaseOrderer);
		}
	}

	public static class TestCollectionOrderer
	{
		[Fact]
		public static async ValueTask DefaultTestCollectionOrderer()
		{
			var runner = TestableTestAssemblyRunner.Create();

			await runner.RunAsync();

			Assert.IsType<DefaultTestCollectionOrderer>(runner.DefaultTestCollectionOrderer);
		}

		[Fact]
		public static async ValueTask OrdererUsedToOrderTestCollections()
		{
			var collection1 = Mocks.TestCollection(displayName: "AAA", uniqueID: "collection-1");
			var testCase1a = TestCaseForTestCollection(collection1);
			var testCase1b = TestCaseForTestCollection(collection1);
			var collection2 = Mocks.TestCollection(displayName: "ZZZZ", uniqueID: "collection-2");
			var testCase2a = TestCaseForTestCollection(collection2);
			var testCase2b = TestCaseForTestCollection(collection2);
			var collection3 = Mocks.TestCollection(displayName: "MM", uniqueID: "collection-3");
			var testCase3a = TestCaseForTestCollection(collection3);
			var testCase3b = TestCaseForTestCollection(collection3);
			var testCases = new[] { testCase1a, testCase3a, testCase2a, testCase3b, testCase2b, testCase1b };
			var runner = TestableTestAssemblyRunner.Create(testCases: testCases, testCollectionOrderer: new DescendingDisplayNameCollectionOrderer());

			await runner.RunAsync();

			Assert.Collection(
				runner.CollectionsRun,
				collection =>
				{
					Assert.Same(collection2, collection.Item1);
					Assert.Equal(new[] { testCase2a, testCase2b }, collection.Item2);
				},
				collection =>
				{
					Assert.Same(collection3, collection.Item1);
					Assert.Equal(new[] { testCase3a, testCase3b }, collection.Item2);
				},
				collection =>
				{
					Assert.Same(collection1, collection.Item1);
					Assert.Equal(new[] { testCase1a, testCase1b }, collection.Item2);
				}
			);
		}

		class DescendingDisplayNameCollectionOrderer : ITestCollectionOrderer
		{
			public IReadOnlyCollection<_ITestCollection> OrderTestCollections(IReadOnlyCollection<_ITestCollection> TestCollections) =>
				TestCollections
					.OrderByDescending(c => c.DisplayName)
					.CastOrToReadOnlyCollection();
		}

		[Fact]
		public static async ValueTask TestCaseOrdererWhichThrowsLogsMessageAndDoesNotReorderTestCollections()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;

			var collection1 = Mocks.TestCollection(displayName: "AAA", uniqueID: "collection-1");
			var testCase1 = TestCaseForTestCollection(collection1);
			var collection2 = Mocks.TestCollection(displayName: "ZZZZ", uniqueID: "collection-2");
			var testCase2 = TestCaseForTestCollection(collection2);
			var collection3 = Mocks.TestCollection(displayName: "MM", uniqueID: "collection-3");
			var testCase3 = TestCaseForTestCollection(collection3);
			var testCases = new[] { testCase1, testCase2, testCase3 };
			var runner = TestableTestAssemblyRunner.Create(testCases: testCases, testCollectionOrderer: new ThrowingCollectionOrderer());

			await runner.RunAsync();

			Assert.Collection(
				runner.CollectionsRun,
				collection => Assert.Same(collection1, collection.Item1),
				collection => Assert.Same(collection2, collection.Item1),
				collection => Assert.Same(collection3, collection.Item1)
			);
			var diagnosticMessage = Assert.Single(spy.Messages.OfType<_DiagnosticMessage>());
			Assert.StartsWith("Test collection orderer 'TestAssemblyRunnerTests+TestCollectionOrderer+ThrowingCollectionOrderer' threw 'System.DivideByZeroException' during ordering: Attempted to divide by zero.", diagnosticMessage.Message);
		}

		class ThrowingCollectionOrderer : ITestCollectionOrderer
		{
			public IReadOnlyCollection<_ITestCollection> OrderTestCollections(IReadOnlyCollection<_ITestCollection> testCollections)
			{
				throw new DivideByZeroException();
			}
		}
	}

	class TestableTestAssemblyRunnerContext : TestAssemblyRunnerContext<_ITestCase>
	{
		public TestableTestAssemblyRunnerContext(
			_ITestAssembly testAssembly,
			IReadOnlyCollection<_ITestCase> testCases,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions) :
				base(testAssembly, testCases, executionMessageSink, executionOptions)
		{ }

		public override string TestFrameworkDisplayName =>
			"The test framework display name";

		public override string TestFrameworkEnvironment =>
			"The test framework environment";

		// Use the sync message bus, so that we can immediately react to cancellations
		protected override IMessageBus CreateMessageBus() =>
			new SynchronousMessageBus(ExecutionMessageSink);
	}

	class TestableTestAssemblyRunner : TestAssemblyRunner<TestableTestAssemblyRunnerContext, _ITestCase>
	{
		readonly bool cancelInRunTestCollectionAsync;
		readonly _IMessageSink executionMessageSink;
		readonly _ITestFrameworkExecutionOptions executionOptions;
		readonly RunSummary result;
		readonly _ITestCase[] testCases;
		readonly ITestCollectionOrderer? testCollectionOrderer;

		public List<Tuple<_ITestCollection, IReadOnlyCollection<_ITestCase>>> CollectionsRun = new();
		public Action<ExceptionAggregator> AfterTestAssemblyStarting_Callback = _ => { };
		public bool AfterTestAssemblyStarting_Called;
		public TestContext? AfterTestAssemblyStarting_Context;
		public Action<ExceptionAggregator> BeforeTestAssemblyFinished_Callback = _ => { };
		public bool BeforeTestAssemblyFinished_Called;
		public TestContext? BeforeTestAssemblyFinished_Context;
		public ITestCaseOrderer? DefaultTestCaseOrderer;
		public ITestCollectionOrderer? DefaultTestCollectionOrderer;
		public Exception? RunTestCollectionAsync_AggregatorResult;
		public TestContext? RunTestCollectionAsync_Context;
		public _ITestAssembly? TestAssembly;

		TestableTestAssemblyRunner(
			_ITestCase[] testCases,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions,
			RunSummary result,
			bool cancelInRunTestCollectionAsync,
			ITestCollectionOrderer? testCollectionOrderer)
		{
			this.testCases = testCases;
			this.executionMessageSink = executionMessageSink;
			this.executionOptions = executionOptions;
			this.result = result;
			this.cancelInRunTestCollectionAsync = cancelInRunTestCollectionAsync;
			this.testCollectionOrderer = testCollectionOrderer;
		}

		public static TestableTestAssemblyRunner Create(
			_IMessageSink? executionMessageSink = null,
			RunSummary? result = null,
			_ITestCase[]? testCases = null,
			_ITestFrameworkExecutionOptions? executionOptions = null,
			bool cancelInRunTestCollectionAsync = false,
			ITestCollectionOrderer? testCollectionOrderer = null) =>
				new(
					testCases ?? new[] { Substitute.For<_ITestCase>() },  // Need at least one so it calls RunTestCollectionAsync
					executionMessageSink ?? SpyMessageSink.Create(),
					executionOptions ?? _TestFrameworkOptions.ForExecution(),
					result ?? new RunSummary(),
					cancelInRunTestCollectionAsync,
					testCollectionOrderer
				);


		protected override ITestCollectionOrderer GetTestCollectionOrderer(TestableTestAssemblyRunnerContext ctxt) =>
			testCollectionOrderer ?? base.GetTestCollectionOrderer(ctxt);

		protected override ValueTask AfterTestAssemblyStartingAsync(TestableTestAssemblyRunnerContext ctxt)
		{
			AfterTestAssemblyStarting_Called = true;
			AfterTestAssemblyStarting_Context = TestContext.Current;
			AfterTestAssemblyStarting_Callback(ctxt.Aggregator);
			return default;
		}

		protected override ValueTask BeforeTestAssemblyFinishedAsync(TestableTestAssemblyRunnerContext ctxt)
		{
			BeforeTestAssemblyFinished_Called = true;
			BeforeTestAssemblyFinished_Context = TestContext.Current;
			BeforeTestAssemblyFinished_Callback(ctxt.Aggregator);
			return default;
		}

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new TestableTestAssemblyRunnerContext(Mocks.TestAssembly(Assembly.GetExecutingAssembly()), testCases, executionMessageSink, executionOptions);
			await ctxt.InitializeAsync();

			DefaultTestCaseOrderer = GetTestCaseOrderer(ctxt);
			DefaultTestCollectionOrderer = GetTestCollectionOrderer(ctxt);
			TestAssembly = ctxt.TestAssembly;

			return await RunAsync(ctxt);
		}

		protected override ValueTask<RunSummary> RunTestCollectionAsync(
			TestableTestAssemblyRunnerContext ctxt,
			_ITestCollection testCollection,
			IReadOnlyCollection<_ITestCase> testCases)
		{
			if (cancelInRunTestCollectionAsync)
				ctxt.CancellationTokenSource.Cancel();

			RunTestCollectionAsync_AggregatorResult = ctxt.Aggregator.ToException();
			RunTestCollectionAsync_Context = TestContext.Current;
			CollectionsRun.Add(Tuple.Create(testCollection, testCases));
			return new(result);
		}
	}

	static _ITestCase TestCaseForTestCollection(_ITestCollection? collection = null)
	{
		collection ??= Mocks.TestCollection();

		var result = Substitute.For<_ITestCase, InterfaceProxy<_ITestCase>>();
		result.TestCollection.Returns(collection);
		return result;
	}
}
