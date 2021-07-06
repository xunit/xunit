using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class TestAssemblyRunnerTests
{
	public class CreateMessageBus
	{
		[Fact]
		public static async ValueTask DefaultMessageBus()
		{
			await using var runner = TestableTestAssemblyRunner.Create();

			using var messageBus = runner.CreateMessageBus_Public();

			Assert.IsType<MessageBus>(messageBus);
		}

		[Fact]
		public static async ValueTask SyncMessageBusOption()
		{
			var executionOptions = _TestFrameworkOptions.ForExecution();
			executionOptions.SetSynchronousMessageReporting(true);
			await using var runner = TestableTestAssemblyRunner.Create(executionOptions: executionOptions);

			using var messageBus = runner.CreateMessageBus_Public();

			Assert.IsType<SynchronousMessageBus>(messageBus);
		}
	}

	public class RunAsync
	{
		[Fact]
		public static async ValueTask Messages()
		{
			var summary = new RunSummary { Total = 4, Failed = 2, Skipped = 1, Time = 21.12m };
			var messages = new List<_MessageSinkMessage>();
			var messageSink = SpyMessageSink.Create(messages: messages);
			await using var runner = TestableTestAssemblyRunner.Create(messageSink, summary);
			var thisAssembly = Assembly.GetExecutingAssembly();

			var result = await runner.RunAsync();

			Assert.Equal(4, result.Total);
			Assert.Equal(2, result.Failed);
			Assert.Equal(1, result.Skipped);
			Assert.NotEqual(21.12m, result.Time);  // Uses clock time, not result time
			Assert.Collection(
				messages,
				msg =>
				{
					var starting = Assert.IsAssignableFrom<_TestAssemblyStarting>(msg);
#if NETFRAMEWORK
					Assert.Equal(thisAssembly.GetLocalCodeBase(), starting.AssemblyPath);
					Assert.Equal(runner.TestAssembly.ConfigFileName, starting.ConfigFilePath);
					Assert.Equal(".NETFramework,Version=v4.7.2", starting.TargetFramework);
#else
					Assert.Equal(".NETCoreApp,Version=v2.1", starting.TargetFramework);
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
					Assert.Equal(4, finished.TestsRun);
					Assert.Equal(1, finished.TestsSkipped);
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
			await using var runner = TestableTestAssemblyRunner.Create(messageSink);

			await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync());

			var starting = Assert.Single(messages);
			Assert.IsAssignableFrom<_TestAssemblyStarting>(starting);
			Assert.Empty(runner.CollectionsRun);
		}

		[Fact]
		public static async ValueTask FailureInAfterTestAssemblyStarting_GivesErroredAggregatorToTestCollectionRunner_NoCleanupFailureMessage()
		{
			var messages = new List<_MessageSinkMessage>();
			var messageSink = SpyMessageSink.Create(messages: messages);
			await using var runner = TestableTestAssemblyRunner.Create(messageSink);
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
			await using var runner = TestableTestAssemblyRunner.Create(messageSink, testCases: testCases);
			var startingException = new DivideByZeroException();
			var finishedException = new InvalidOperationException();
			runner.AfterTestAssemblyStarting_Callback = aggregator => aggregator.Add(startingException);
			runner.BeforeTestAssemblyFinished_Callback = aggregator => aggregator.Add(finishedException);

			await runner.RunAsync();

			var assemblyStarting = Assert.Single(messages.OfType<_TestAssemblyStarting>());
			var cleanupFailure = Assert.Single(messages.OfType<_TestAssemblyCleanupFailure>());
#if NETFRAMEWORK
			Assert.Equal(thisAssembly.GetLocalCodeBase(), assemblyStarting.AssemblyPath);
			Assert.Equal(runner.TestAssembly.ConfigFileName, assemblyStarting.ConfigFilePath);
#endif
			Assert.Equal(typeof(InvalidOperationException).FullName, cleanupFailure.ExceptionTypes.Single());
		}

		[Fact]
		public static async ValueTask Cancellation_TestAssemblyStarting_DoesNotCallExtensibilityCallbacks()
		{
			var messageSink = SpyMessageSink.Create(msg => !(msg is _TestAssemblyStarting));
			await using var runner = TestableTestAssemblyRunner.Create(messageSink);

			await runner.RunAsync();

			Assert.False(runner.AfterTestAssemblyStarting_Called);
			Assert.False(runner.BeforeTestAssemblyFinished_Called);
		}

		[Fact]
		public static async ValueTask Cancellation_TestAssemblyFinished_CallsCallExtensibilityCallbacks()
		{
			var messageSink = SpyMessageSink.Create(msg => !(msg is _TestAssemblyFinished));
			await using var runner = TestableTestAssemblyRunner.Create(messageSink);

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
			await using var runner = TestableTestAssemblyRunner.Create(testCases: new[] { testCase1a, testCase2a, testCase2b, testCase1b });

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
		public static async void SignalingCancellationStopsRunningCollections()
		{
			var collection1 = Mocks.TestCollection();
			var testCase1 = TestCaseForTestCollection(collection1);
			var collection2 = Mocks.TestCollection();
			var testCase2 = TestCaseForTestCollection(collection2);
			await using var runner = TestableTestAssemblyRunner.Create(testCases: new[] { testCase1, testCase2 }, cancelInRunTestCollectionAsync: true);

			await runner.RunAsync();

			Assert.Single(runner.CollectionsRun);
		}
	}

	public class TestCaseOrderer
	{
		[Fact]
		public static async ValueTask DefaultTestCaseOrderer()
		{
			await using var runner = TestableTestAssemblyRunner.Create();

			Assert.IsType<DefaultTestCaseOrderer>(runner.TestCaseOrderer);
		}
	}

	public class TestCollectionOrderer
	{
		[Fact]
		public static async ValueTask DefaultTestCaseOrderer()
		{
			await using var runner = TestableTestAssemblyRunner.Create();

			Assert.IsType<DefaultTestCollectionOrderer>(runner.TestCollectionOrderer);
		}

		[Fact]
		public static async ValueTask OrdererUsedToOrderTestCases()
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
			await using var runner = TestableTestAssemblyRunner.Create(testCases: testCases);
			runner.TestCollectionOrderer = new MyTestCollectionOrderer();

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

		class MyTestCollectionOrderer : ITestCollectionOrderer
		{
			public IReadOnlyCollection<_ITestCollection> OrderTestCollections(IReadOnlyCollection<_ITestCollection> TestCollections) =>
				TestCollections
					.OrderByDescending(c => c.DisplayName)
					.CastOrToReadOnlyCollection();
		}

		[Fact]
		public static async ValueTask TestCaseOrdererWhichThrowsLogsMessageAndDoesNotReorderTests()
		{
			var collection1 = Mocks.TestCollection(displayName: "AAA", uniqueID: "collection-1");
			var testCase1 = TestCaseForTestCollection(collection1);
			var collection2 = Mocks.TestCollection(displayName: "ZZZZ", uniqueID: "collection-2");
			var testCase2 = TestCaseForTestCollection(collection2);
			var collection3 = Mocks.TestCollection(displayName: "MM", uniqueID: "collection-3");
			var testCase3 = TestCaseForTestCollection(collection3);
			var testCases = new[] { testCase1, testCase2, testCase3 };
			await using var runner = TestableTestAssemblyRunner.Create(testCases: testCases);
			runner.TestCollectionOrderer = new ThrowingOrderer();

			await runner.RunAsync();

			Assert.Collection(
				runner.CollectionsRun,
				collection => Assert.Same(collection1, collection.Item1),
				collection => Assert.Same(collection2, collection.Item1),
				collection => Assert.Same(collection3, collection.Item1)
			);
			var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<_DiagnosticMessage>());
			Assert.StartsWith("Test collection orderer 'TestAssemblyRunnerTests+TestCollectionOrderer+ThrowingOrderer' threw 'System.DivideByZeroException' during ordering: Attempted to divide by zero.", diagnosticMessage.Message);
		}

		class ThrowingOrderer : ITestCollectionOrderer
		{
			public IReadOnlyCollection<_ITestCollection> OrderTestCollections(IReadOnlyCollection<_ITestCollection> testCollections)
			{
				throw new DivideByZeroException();
			}
		}
	}

	class TestableTestAssemblyRunner : TestAssemblyRunner<_ITestCase>
	{
		readonly bool cancelInRunTestCollectionAsync;
		readonly RunSummary result;

		public List<Tuple<_ITestCollection, IReadOnlyCollection<_ITestCase>>> CollectionsRun = new();
		public Action<ExceptionAggregator> AfterTestAssemblyStarting_Callback = _ => { };
		public bool AfterTestAssemblyStarting_Called;
		public Action<ExceptionAggregator> BeforeTestAssemblyFinished_Callback = _ => { };
		public bool BeforeTestAssemblyFinished_Called;
		public List<_MessageSinkMessage> DiagnosticMessages;
		public Exception? RunTestCollectionAsync_AggregatorResult;

		TestableTestAssemblyRunner(
			_ITestAssembly testAssembly,
			IReadOnlyCollection<_ITestCase> testCases,
			List<_MessageSinkMessage> diagnosticMessages,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions,
			RunSummary result,
			bool cancelInRunTestCollectionAsync)
				: base(testAssembly, testCases, SpyMessageSink.Create(messages: diagnosticMessages), executionMessageSink, executionOptions)
		{
			DiagnosticMessages = diagnosticMessages;

			this.result = result;
			this.cancelInRunTestCollectionAsync = cancelInRunTestCollectionAsync;
		}

		public new _ITestAssembly TestAssembly => base.TestAssembly;

		public static TestableTestAssemblyRunner Create(
			_IMessageSink? executionMessageSink = null,
			RunSummary? result = null,
			_ITestCase[]? testCases = null,
			_ITestFrameworkExecutionOptions? executionOptions = null,
			bool cancelInRunTestCollectionAsync = false)
		{
			return new TestableTestAssemblyRunner(
				Mocks.TestAssembly(Assembly.GetExecutingAssembly()),
				testCases ?? new[] { Substitute.For<_ITestCase>() },  // Need at least one so it calls RunTestCollectionAsync
				new List<_MessageSinkMessage>(),
				executionMessageSink ?? SpyMessageSink.Create(),
				executionOptions ?? _TestFrameworkOptions.ForExecution(),
				result ?? new RunSummary(),
				cancelInRunTestCollectionAsync
			);
		}

		public new ITestCaseOrderer TestCaseOrderer => base.TestCaseOrderer;

		public new ITestCollectionOrderer TestCollectionOrderer
		{
			get { return base.TestCollectionOrderer; }
			set { base.TestCollectionOrderer = value; }
		}

		public IMessageBus CreateMessageBus_Public() => base.CreateMessageBus();

		protected override IMessageBus CreateMessageBus()
		{
			// Use the sync message bus, so that we can immediately react to cancellations.
			return new SynchronousMessageBus(ExecutionMessageSink);
		}

		protected override string GetTestFrameworkDisplayName() => "The test framework display name";

		protected override string GetTestFrameworkEnvironment() => "The test framework environment";

		protected override Task AfterTestAssemblyStartingAsync()
		{
			AfterTestAssemblyStarting_Called = true;
			AfterTestAssemblyStarting_Callback(Aggregator);
			return Task.CompletedTask;
		}

		protected override Task BeforeTestAssemblyFinishedAsync()
		{
			BeforeTestAssemblyFinished_Called = true;
			BeforeTestAssemblyFinished_Callback(Aggregator);
			return Task.CompletedTask;
		}

		protected override Task<RunSummary> RunTestCollectionAsync(
			IMessageBus messageBus,
			_ITestCollection testCollection,
			IReadOnlyCollection<_ITestCase> testCases,
			CancellationTokenSource cancellationTokenSource)
		{
			if (cancelInRunTestCollectionAsync)
				cancellationTokenSource.Cancel();

			RunTestCollectionAsync_AggregatorResult = Aggregator.ToException();
			CollectionsRun.Add(Tuple.Create(testCollection, testCases));
			return Task.FromResult(result);
		}
	}

	static _ITestCase TestCaseForTestCollection(_ITestCollection? collection = null)
	{
		collection ??= Mocks.TestCollection();

		var result = Substitute.For<_ITestCase, InterfaceProxy<_ITestCase>>();
		result.TestMethod.TestClass.TestCollection.Returns(collection);
		return result;
	}
}
