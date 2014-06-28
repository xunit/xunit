﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A base class that provides default behavior when running tests in an assembly. It groups the tests
    /// by test collection, and then runs the individual test collections.
    /// </summary>
    /// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
    /// derive from <see cref="ITestCase"/>.</typeparam>
    public abstract class TestAssemblyRunner<TTestCase> : IDisposable
        where TTestCase : ITestCase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyRunner{TTestCase}"/> class.
        /// </summary>
        /// <param name="testAssembly">The assembly that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="messageSink">The message sink to report run status to.</param>
        /// <param name="executionOptions">The user's requested execution options.</param>
        public TestAssemblyRunner(ITestAssembly testAssembly,
                                  IEnumerable<TTestCase> testCases,
                                  IMessageSink messageSink,
                                  ITestFrameworkOptions executionOptions)
        {
            TestAssembly = testAssembly;
            TestCases = testCases;
            MessageSink = messageSink;
            ExecutionOptions = executionOptions;
            TestCaseOrderer = new DefaultTestCaseOrderer();
            Aggregator = new ExceptionAggregator();
        }

        /// <summary>
        /// Gets or sets the exception aggregator used to run code and collect exceptions.
        /// </summary>
        protected ExceptionAggregator Aggregator { get; set; }

        /// <summary>
        /// Gets or sets the user's requested execution options.
        /// </summary>
        protected ITestFrameworkOptions ExecutionOptions { get; set; }

        /// <summary>
        /// Gets or sets the message sink to report run status to.
        /// </summary>
        protected IMessageSink MessageSink { get; set; }

        /// <summary>
        /// Gets or sets the assembly that contains the tests to be run.
        /// </summary>
        protected ITestAssembly TestAssembly { get; set; }

        /// <summary>
        /// Gets or sets the test case orderer that will be used to decide how to order the test.
        /// </summary>
        protected ITestCaseOrderer TestCaseOrderer { get; set; }

        /// <summary>
        /// Gets or sets the test cases to be run.
        /// </summary>
        protected IEnumerable<TTestCase> TestCases { get; set; }

        /// <inheritdoc/>
        public virtual void Dispose() { }

        /// <summary>
        /// Override this to provide the display name for the test framework (f.e., "xUnit.net 2.0").
        /// This value is placed into <see cref="ITestAssemblyStarting.TestFrameworkDisplayName"/>.
        /// </summary>
        protected abstract string GetTestFrameworkDisplayName();

        /// <summary>
        /// Override this to provide the environment information (f.e., "32-bit .NET 4.0"). This value is
        /// placed into <see cref="ITestAssemblyStarting.TestEnvironment"/>.
        /// </summary>
        protected abstract string GetTestFrameworkEnvironment();

        /// <summary>
        /// This method is called just after <see cref="ITestAssemblyStarting"/> it sent, but before any test collections are run.
        /// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
        /// </summary>
        protected virtual void OnAssemblyStarted() { }

        /// <summary>
        /// This method is called just before <see cref="ITestAssemblyFinished"/> is sent.
        /// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
        /// </summary>
        protected virtual void OnAssemblyFinishing() { }

        /// <summary>
        /// Creates the message bus to be used for test execution. By default, it inspects
        /// the options for the <see cref="TestOptionsNames.Execution.SynchronousMessageReporting"/>
        /// flag, and if present, creates a message bus that ensures all messages are delivered
        /// on the same thread.
        /// </summary>
        /// <returns>The message bus.</returns>
        protected virtual IMessageBus CreateMessageBus()
        {
            if (ExecutionOptions.GetValue(TestOptionsNames.Execution.SynchronousMessageReporting, false))
                return new SynchronousMessageBus(MessageSink);

            return new MessageBus(MessageSink);
        }

        /// <summary>
        /// Runs the tests in the test assembly.
        /// </summary>
        /// <returns>Returns summary information about the tests that were run.</returns>
        public async Task<RunSummary> RunAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var totalSummary = new RunSummary();
#if !WINDOWS_PHONE_APP
            var currentDirectory = Directory.GetCurrentDirectory();
#endif
            var testFrameworkEnvironment = GetTestFrameworkEnvironment();
            var testFrameworkDisplayName = GetTestFrameworkDisplayName();

            using (var messageBus = CreateMessageBus())
            {
                try
                {
                    
#if !WINDOWS_PHONE_APP
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(TestAssembly.Assembly.AssemblyPath));
#endif

                    if (messageBus.QueueMessage(new TestAssemblyStarting(TestCases.Cast<ITestCase>(), TestAssembly, DateTime.Now, testFrameworkEnvironment, testFrameworkDisplayName)))
                    {
                        OnAssemblyStarted();

                        var masterStopwatch = Stopwatch.StartNew();
                        totalSummary = await RunTestCollectionsAsync(messageBus, cancellationTokenSource);
                        // Want clock time, not aggregated run time
                        totalSummary.Time = (decimal)masterStopwatch.Elapsed.TotalSeconds;

                        Aggregator.Clear();
                        OnAssemblyFinishing();

                        if (Aggregator.HasExceptions)
                            messageBus.QueueMessage(new TestAssemblyCleanupFailure(TestCases.Cast<ITestCase>(), TestAssembly, Aggregator.ToException()));
                    }
                }
                finally
                {
                    
                    messageBus.QueueMessage(new TestAssemblyFinished(TestCases.Cast<ITestCase>(), TestAssembly, totalSummary.Time, totalSummary.Total, totalSummary.Failed, totalSummary.Skipped));
#if !WINDOWS_PHONE_APP
                    Directory.SetCurrentDirectory(currentDirectory);
#endif
                }
            }

            return totalSummary;
        }

        /// <summary>
        /// Runs the list of test collections. By default, groups the tests by collection and runs them synchronously.
        /// </summary>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        /// <returns>Returns summary information about the tests that were run.</returns>
        protected virtual async Task<RunSummary> RunTestCollectionsAsync(IMessageBus messageBus, CancellationTokenSource cancellationTokenSource)
        {
            var summary = new RunSummary();

            foreach (var collectionGroup in TestCases.Cast<TTestCase>().GroupBy(tc => tc.TestMethod.TestClass.TestCollection, TestCollectionComparer.Instance))
            {
                summary.Aggregate(await RunTestCollectionAsync(messageBus, collectionGroup.Key, collectionGroup, cancellationTokenSource));
                if (cancellationTokenSource.IsCancellationRequested)
                    break;
            }

            return summary;
        }

        /// <summary>
        /// Override this method to run the tests in an individual test collection.
        /// </summary>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="testCollection">The test collection that is being run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        /// <returns>Returns summary information about the tests that were run.</returns>
        protected abstract Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<TTestCase> testCases, CancellationTokenSource cancellationTokenSource);
    }
}
