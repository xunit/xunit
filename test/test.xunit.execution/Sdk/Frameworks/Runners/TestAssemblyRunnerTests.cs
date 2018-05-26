using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestAssemblyRunnerTests
{
    public class CreateMessageBus
    {
        [Fact]
        public static void DefaultMessageBus()
        {
            var runner = TestableTestAssemblyRunner.Create();

            var messageBus = runner.CreateMessageBus_Public();

            Assert.IsType<MessageBus>(messageBus);
        }

        [Fact]
        public static void SyncMessageBusOption()
        {
            var executionOptions = TestFrameworkOptions.ForExecution();
            executionOptions.SetSynchronousMessageReporting(true);
            var runner = TestableTestAssemblyRunner.Create(executionOptions: executionOptions);

            var messageBus = runner.CreateMessageBus_Public();

            Assert.IsType<SynchronousMessageBus>(messageBus);
        }
    }

    public class RunAsync
    {
        [Fact]
        public static async void Messages()
        {
            var summary = new RunSummary { Total = 4, Failed = 2, Skipped = 1, Time = 21.12m };
            var messages = new List<IMessageSinkMessage>();
            var messageSink = SpyMessageSink.Create(messages: messages);
            var runner = TestableTestAssemblyRunner.Create(messageSink, summary);
            var thisAssembly = Assembly.GetExecutingAssembly();
            var thisAppDomain = AppDomain.CurrentDomain;

            var result = await runner.RunAsync();

            Assert.Equal(4, result.Total);
            Assert.Equal(2, result.Failed);
            Assert.Equal(1, result.Skipped);
            Assert.NotEqual(21.12m, result.Time);  // Uses clock time, not result time
            Assert.Collection(messages,
                msg =>
                {
                    var starting = Assert.IsAssignableFrom<ITestAssemblyStarting>(msg);
#if NETFRAMEWORK
                    Assert.Equal(thisAssembly.GetLocalCodeBase(), starting.TestAssembly.Assembly.AssemblyPath);
                    Assert.Equal(thisAppDomain.SetupInformation.ConfigurationFile, starting.TestAssembly.ConfigFileName);
#endif
                    Assert.InRange(starting.StartTime, DateTime.Now.AddMinutes(-15), DateTime.Now);
                    Assert.Equal("The test framework environment", starting.TestEnvironment);
                    Assert.Equal("The test framework display name", starting.TestFrameworkDisplayName);
                },
                msg =>
                {
                    var finished = Assert.IsAssignableFrom<ITestAssemblyFinished>(msg);
                    Assert.Equal(4, finished.TestsRun);
                    Assert.Equal(2, finished.TestsFailed);
                    Assert.Equal(1, finished.TestsSkipped);
                    Assert.Equal(result.Time, finished.ExecutionTime);
                }
            );
        }

        [Fact]
        public static async void FailureInQueueOfTestAssemblyStarting_DoesNotQueueTestAssemblyFinished_DoesNotRunTestCollections()
        {
            var messages = new List<IMessageSinkMessage>();
            var messageSink = Substitute.For<IMessageSink>();
            messageSink.OnMessage(null)
                       .ReturnsForAnyArgs(callInfo =>
                       {
                           var msg = callInfo.Arg<IMessageSinkMessage>();
                           messages.Add(msg);

                           if (msg is ITestAssemblyStarting)
                               throw new InvalidOperationException();

                           return true;
                       });
            var runner = TestableTestAssemblyRunner.Create(messageSink);

            await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync());

            var starting = Assert.Single(messages);
            Assert.IsAssignableFrom<ITestAssemblyStarting>(starting);
            Assert.Empty(runner.CollectionsRun);
        }

        [Fact]
        public static async void FailureInAfterTestAssemblyStarting_GivesErroredAggregatorToTestCollectionRunner_NoCleanupFailureMessage()
        {
            var messages = new List<IMessageSinkMessage>();
            var messageSink = SpyMessageSink.Create(messages: messages);
            var runner = TestableTestAssemblyRunner.Create(messageSink);
            var ex = new DivideByZeroException();
            runner.AfterTestAssemblyStarting_Callback = aggregator => aggregator.Add(ex);

            await runner.RunAsync();

            Assert.Same(ex, runner.RunTestCollectionAsync_AggregatorResult);
            Assert.Empty(messages.OfType<ITestAssemblyCleanupFailure>());
        }

        [Fact]
        public static async void FailureInBeforeTestAssemblyFinished_ReportsCleanupFailure_DoesNotIncludeExceptionsFromAfterTestAssemblyStarting()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var thisAppDomain = AppDomain.CurrentDomain;
            var messages = new List<IMessageSinkMessage>();
            var messageSink = SpyMessageSink.Create(messages: messages);
            var testCases = new[] { Mocks.TestCase() };
            var runner = TestableTestAssemblyRunner.Create(messageSink, testCases: testCases);
            var startingException = new DivideByZeroException();
            var finishedException = new InvalidOperationException();
            runner.AfterTestAssemblyStarting_Callback = aggregator => aggregator.Add(startingException);
            runner.BeforeTestAssemblyFinished_Callback = aggregator => aggregator.Add(finishedException);

            await runner.RunAsync();

            var cleanupFailure = Assert.Single(messages.OfType<ITestAssemblyCleanupFailure>());
#if NETFRAMEWORK
            Assert.Equal(thisAssembly.GetLocalCodeBase(), cleanupFailure.TestAssembly.Assembly.AssemblyPath);
            Assert.Equal(thisAppDomain.SetupInformation.ConfigurationFile, cleanupFailure.TestAssembly.ConfigFileName);
#endif
            Assert.Equal(testCases, cleanupFailure.TestCases);
            Assert.Equal(typeof(InvalidOperationException).FullName, cleanupFailure.ExceptionTypes.Single());
        }

        [Fact]
        public static async void Cancellation_TestAssemblyStarting_DoesNotCallExtensibilityCallbacks()
        {
            var messageSink = SpyMessageSink.Create(msg => !(msg is ITestAssemblyStarting));
            var runner = TestableTestAssemblyRunner.Create(messageSink);

            await runner.RunAsync();

            Assert.False(runner.AfterTestAssemblyStarting_Called);
            Assert.False(runner.BeforeTestAssemblyFinished_Called);
        }

        [Fact]
        public static async void Cancellation_TestAssemblyFinished_CallsCallExtensibilityCallbacks()
        {
            var messageSink = SpyMessageSink.Create(msg => !(msg is ITestAssemblyFinished));
            var runner = TestableTestAssemblyRunner.Create(messageSink);

            await runner.RunAsync();

            Assert.True(runner.AfterTestAssemblyStarting_Called);
            Assert.True(runner.BeforeTestAssemblyFinished_Called);
        }

        [Fact]
        public static async void TestsAreGroupedByCollection()
        {
            var collection1 = Mocks.TestCollection(displayName: "1");
            var testCase1a = Mocks.TestCase(collection1);
            var testCase1b = Mocks.TestCase(collection1);
            var collection2 = Mocks.TestCollection(displayName: "2");
            var testCase2a = Mocks.TestCase(collection2);
            var testCase2b = Mocks.TestCase(collection2);
            var runner = TestableTestAssemblyRunner.Create(testCases: new[] { testCase1a, testCase2a, testCase2b, testCase1b });

            await runner.RunAsync();

            Assert.Collection(runner.CollectionsRun.OrderBy(c => c.Item1.DisplayName),
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
            var testCase1 = Mocks.TestCase(collection1);
            var collection2 = Mocks.TestCollection();
            var testCase2 = Mocks.TestCase(collection2);
            var runner = TestableTestAssemblyRunner.Create(testCases: new[] { testCase1, testCase2 }, cancelInRunTestCollectionAsync: true);

            await runner.RunAsync();

            Assert.Single(runner.CollectionsRun);
        }
    }

    public class TestCaseOrderer
    {
        [Fact]
        public static void DefaultTestCaseOrderer()
        {
            var runner = TestableTestAssemblyRunner.Create();

            Assert.IsType<DefaultTestCaseOrderer>(runner.TestCaseOrderer);
        }
    }

    public class TestCollectionOrderer
    {
        [Fact]
        public static void DefaultTestCaseOrderer()
        {
            var runner = TestableTestAssemblyRunner.Create();

            Assert.IsType<DefaultTestCollectionOrderer>(runner.TestCollectionOrderer);
        }

        [Fact]
        public static async void OrdererUsedToOrderTestCases()
        {
            var collection1 = Mocks.TestCollection(displayName: "AAA");
            var testCase1a = Mocks.TestCase(collection1);
            var testCase1b = Mocks.TestCase(collection1);
            var collection2 = Mocks.TestCollection(displayName: "ZZZZ");
            var testCase2a = Mocks.TestCase(collection2);
            var testCase2b = Mocks.TestCase(collection2);
            var collection3 = Mocks.TestCollection(displayName: "MM");
            var testCase3a = Mocks.TestCase(collection3);
            var testCase3b = Mocks.TestCase(collection3);
            var testCases = new[] { testCase1a, testCase3a, testCase2a, testCase3b, testCase2b, testCase1b };
            var runner = TestableTestAssemblyRunner.Create(testCases: testCases);
            runner.TestCollectionOrderer = new MyTestCollectionOrderer();

            await runner.RunAsync();

            Assert.Collection(runner.CollectionsRun,
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
            public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> TestCollections)
            {
                return TestCollections.OrderByDescending(c => c.DisplayName);
            }
        }

        [Fact]
        public static async void TestCaseOrdererWhichThrowsLogsMessageAndDoesNotReorderTests()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            var collection1 = Mocks.TestCollection(displayName: "AAA");
            var testCase1 = Mocks.TestCase(collection1);
            var collection2 = Mocks.TestCollection(displayName: "ZZZZ");
            var testCase2 = Mocks.TestCase(collection2);
            var collection3 = Mocks.TestCollection(displayName: "MM");
            var testCase3 = Mocks.TestCase(collection3);
            var testCases = new[] { testCase1, testCase2, testCase3 };
            var runner = TestableTestAssemblyRunner.Create(testCases: testCases);
            runner.TestCollectionOrderer = new ThrowingOrderer();

            await runner.RunAsync();

            Assert.Collection(runner.CollectionsRun,
                collection => Assert.Same(collection1, collection.Item1),
                collection => Assert.Same(collection2, collection.Item1),
                collection => Assert.Same(collection3, collection.Item1)
            );
            var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<IDiagnosticMessage>());
            Assert.StartsWith("Test collection orderer 'TestAssemblyRunnerTests+TestCollectionOrderer+ThrowingOrderer' threw 'System.DivideByZeroException' during ordering: Attempted to divide by zero.", diagnosticMessage.Message);
        }

        class ThrowingOrderer : ITestCollectionOrderer
        {
            public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
            {
                throw new DivideByZeroException();
            }
        }
    }

    class TestableTestAssemblyRunner : TestAssemblyRunner<ITestCase>
    {
        readonly bool cancelInRunTestCollectionAsync;
        readonly RunSummary result;

        public List<Tuple<ITestCollection, IEnumerable<ITestCase>>> CollectionsRun = new List<Tuple<ITestCollection, IEnumerable<ITestCase>>>();
        public Action<ExceptionAggregator> AfterTestAssemblyStarting_Callback = _ => { };
        public bool AfterTestAssemblyStarting_Called;
        public Action<ExceptionAggregator> BeforeTestAssemblyFinished_Callback = _ => { };
        public bool BeforeTestAssemblyFinished_Called;
        public List<IMessageSinkMessage> DiagnosticMessages;
        public Exception RunTestCollectionAsync_AggregatorResult;

        TestableTestAssemblyRunner(ITestAssembly testAssembly,
                                   IEnumerable<ITestCase> testCases,
                                   List<IMessageSinkMessage> diagnosticMessages,
                                   IMessageSink executionMessageSink,
                                   ITestFrameworkExecutionOptions executionOptions,
                                   RunSummary result,
                                   bool cancelInRunTestCollectionAsync)
            : base(testAssembly, testCases, SpyMessageSink.Create(messages: diagnosticMessages), executionMessageSink, executionOptions)
        {
            DiagnosticMessages = diagnosticMessages;

            this.result = result;
            this.cancelInRunTestCollectionAsync = cancelInRunTestCollectionAsync;
        }

        public static TestableTestAssemblyRunner Create(IMessageSink executionMessageSink = null,
                                                        RunSummary result = null,
                                                        ITestCase[] testCases = null,
                                                        ITestFrameworkExecutionOptions executionOptions = null,
                                                        bool cancelInRunTestCollectionAsync = false)
        {
            return new TestableTestAssemblyRunner(
                Mocks.TestAssembly(Assembly.GetExecutingAssembly()),
                testCases ?? new[] { Substitute.For<ITestCase>() },  // Need at least one so it calls RunTestCollectionAsync
                new List<IMessageSinkMessage>(),
                executionMessageSink ?? SpyMessageSink.Create(),
                executionOptions ?? TestFrameworkOptions.ForExecution(),
                result ?? new RunSummary(),
                cancelInRunTestCollectionAsync
            );
        }

        public new ITestCaseOrderer TestCaseOrderer
        {
            get { return base.TestCaseOrderer; }
        }

        public new ITestCollectionOrderer TestCollectionOrderer
        {
            get { return base.TestCollectionOrderer; }
            set { base.TestCollectionOrderer = value; }
        }

        public IMessageBus CreateMessageBus_Public()
        {
            return base.CreateMessageBus();
        }

        protected override IMessageBus CreateMessageBus()
        {
            // Use the sync message bus, so that we can immediately react to cancellations.
            return new SynchronousMessageBus(ExecutionMessageSink);
        }

        protected override string GetTestFrameworkDisplayName()
        {
            return "The test framework display name";
        }

        protected override string GetTestFrameworkEnvironment()
        {
            return "The test framework environment";
        }

        protected override Task AfterTestAssemblyStartingAsync()
        {
            AfterTestAssemblyStarting_Called = true;
            AfterTestAssemblyStarting_Callback(Aggregator);
            return Task.FromResult(0);
        }

        protected override Task BeforeTestAssemblyFinishedAsync()
        {
            BeforeTestAssemblyFinished_Called = true;
            BeforeTestAssemblyFinished_Callback(Aggregator);
            return Task.FromResult(0);
        }

        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<ITestCase> testCases, CancellationTokenSource cancellationTokenSource)
        {
            if (cancelInRunTestCollectionAsync)
                cancellationTokenSource.Cancel();

            RunTestCollectionAsync_AggregatorResult = Aggregator.ToException();
            CollectionsRun.Add(Tuple.Create(testCollection, testCases));
            return Task.FromResult(result);
        }
    }
}
