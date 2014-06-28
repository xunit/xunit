﻿using System;
using System.Collections.Generic;
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
            var runner = TestableTestAssemblyRunner.Create(options: new XunitExecutionOptions { SynchronousMessageReporting = true });

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
                    Assert.Equal(thisAssembly.GetLocalCodeBase(), starting.TestAssembly.Assembly.AssemblyPath);
                    Assert.Equal(thisAppDomain.SetupInformation.ConfigurationFile, starting.TestAssembly.ConfigFileName);
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
        public static async void FailureInOnTestAssemblyStarted_GivesErroredAggregatorToTestCollectionRunner_NoCleanupFailureMessage()
        {
            var messages = new List<IMessageSinkMessage>();
            var messageSink = SpyMessageSink.Create(messages: messages);
            var runner = TestableTestAssemblyRunner.Create(messageSink);
            var ex = new DivideByZeroException();
            runner.OnAssemblyStarted_Callback = aggregator => aggregator.Add(ex);

            await runner.RunAsync();

            Assert.Same(ex, runner.RunTestCollectionAsync_AggregatorResult);
            Assert.Empty(messages.OfType<ITestAssemblyCleanupFailure>());
        }

        [Fact]
        public static async void FailureInOnTestAssemblyFinishing_ReportsCleanupFailure_DoesNotIncludeExceptionsFromTestAssemblyStarted()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var thisAppDomain = AppDomain.CurrentDomain;
            var messages = new List<IMessageSinkMessage>();
            var messageSink = SpyMessageSink.Create(messages: messages);
            var testCases = new[] { Mocks.TestCase() };
            var runner = TestableTestAssemblyRunner.Create(messageSink, testCases: testCases);
            var startedException = new DivideByZeroException();
            var finishingException = new InvalidOperationException();
            runner.OnAssemblyStarted_Callback = aggregator => aggregator.Add(startedException);
            runner.OnAssemblyFinishing_Callback = aggregator => aggregator.Add(finishingException);

            await runner.RunAsync();

            var cleanupFailure = Assert.Single(messages.OfType<ITestAssemblyCleanupFailure>());
            Assert.Equal(thisAssembly.GetLocalCodeBase(), cleanupFailure.TestAssembly.Assembly.AssemblyPath);
            Assert.Equal(thisAppDomain.SetupInformation.ConfigurationFile, cleanupFailure.TestAssembly.ConfigFileName);
            Assert.Same(testCases, cleanupFailure.TestCases);
            Assert.Equal(typeof(InvalidOperationException).FullName, cleanupFailure.ExceptionTypes.Single());
        }

        [Fact]
        public static async void Cancellation_TestAssemblyStarting_DoesNotCallExtensibilityCallbacks()
        {
            var messageSink = SpyMessageSink.Create(msg => !(msg is ITestAssemblyStarting));
            var runner = TestableTestAssemblyRunner.Create(messageSink);

            await runner.RunAsync();

            Assert.False(runner.OnAssemblyStarted_Called);
            Assert.False(runner.OnAssemblyFinishing_Called);
        }

        [Fact]
        public static async void Cancellation_TestAssemblyFinished_CallsCallExtensibilityCallbacks()
        {
            var messageSink = SpyMessageSink.Create(msg => !(msg is ITestAssemblyFinished));
            var runner = TestableTestAssemblyRunner.Create(messageSink);

            await runner.RunAsync();

            Assert.True(runner.OnAssemblyStarted_Called);
            Assert.True(runner.OnAssemblyFinishing_Called);
        }

        [Fact]
        public static async void TestsAreGroupedByCollection()
        {
            var collection1 = Mocks.TestCollection();
            var testCase1a = Mocks.TestCase(collection1);
            var testCase1b = Mocks.TestCase(collection1);
            var collection2 = Mocks.TestCollection();
            var testCase2a = Mocks.TestCase(collection2);
            var testCase2b = Mocks.TestCase(collection2);
            var runner = TestableTestAssemblyRunner.Create(testCases: new[] { testCase1a, testCase2a, testCase2b, testCase1b });

            await runner.RunAsync();

            Assert.Collection(runner.CollectionsRun,
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

            var tuple = Assert.Single(runner.CollectionsRun);
            Assert.Same(collection1, tuple.Item1);
        }
    }

    class TestableTestAssemblyRunner : TestAssemblyRunner<ITestCase>
    {
        readonly bool cancelInRunTestCollectionAsync;
        readonly RunSummary result;

        public List<Tuple<ITestCollection, IEnumerable<ITestCase>>> CollectionsRun = new List<Tuple<ITestCollection, IEnumerable<ITestCase>>>();
        public Action<ExceptionAggregator> OnAssemblyFinishing_Callback = _ => { };
        public bool OnAssemblyFinishing_Called;
        public Action<ExceptionAggregator> OnAssemblyStarted_Callback = _ => { };
        public bool OnAssemblyStarted_Called;
        public Exception RunTestCollectionAsync_AggregatorResult;

        TestableTestAssemblyRunner(ITestAssembly testAssembly,
                                   IEnumerable<ITestCase> testCases,
                                   IMessageSink messageSink,
                                   ITestFrameworkOptions executionOptions,
                                   RunSummary result,
                                   bool cancelInRunTestCollectionAsync)
            : base(testAssembly, testCases, messageSink, executionOptions)
        {
            this.result = result;
            this.cancelInRunTestCollectionAsync = cancelInRunTestCollectionAsync;
        }

        public static TestableTestAssemblyRunner Create(IMessageSink messageSink = null,
                                                        RunSummary result = null,
                                                        ITestCase[] testCases = null,
                                                        ITestFrameworkOptions options = null,
                                                        bool cancelInRunTestCollectionAsync = false)
        {
            return new TestableTestAssemblyRunner(
                Mocks.TestAssembly(Assembly.GetExecutingAssembly()),
                testCases ?? new[] { Substitute.For<ITestCase>() },  // Need at least one so it calls RunTestCollectionAsync
                messageSink ?? SpyMessageSink.Create(),
                options ?? new TestFrameworkOptions(),
                result ?? new RunSummary(),
                cancelInRunTestCollectionAsync
            );
        }

        public IMessageBus CreateMessageBus_Public()
        {
            return base.CreateMessageBus();
        }

        protected override IMessageBus CreateMessageBus()
        {
            // Use the sync message bus, so that we can immediately react to cancellations.
            return new SynchronousMessageBus(MessageSink);
        }

        protected override string GetTestFrameworkDisplayName()
        {
            return "The test framework display name";
        }

        protected override string GetTestFrameworkEnvironment()
        {
            return "The test framework environment";
        }

        protected override void OnAssemblyStarted()
        {
            OnAssemblyStarted_Called = true;
            OnAssemblyStarted_Callback(Aggregator);
        }

        protected override void OnAssemblyFinishing()
        {
            OnAssemblyFinishing_Called = true;
            OnAssemblyFinishing_Callback(Aggregator);
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
