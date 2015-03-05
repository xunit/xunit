using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
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
        /// <param name="diagnosticMessageSink">The message sink to report diagnostic messages to.</param>
        /// <param name="executionMessageSink">The message sink to report run status to.</param>
        /// <param name="executionOptions">The user's requested execution options.</param>
        public TestAssemblyRunner(ITestAssembly testAssembly,
                                  IEnumerable<TTestCase> testCases,
                                  IMessageSink diagnosticMessageSink,
                                  IMessageSink executionMessageSink,
                                  ITestFrameworkExecutionOptions executionOptions)
        {
            TestAssembly = testAssembly;
            TestCases = testCases;
            DiagnosticMessageSink = diagnosticMessageSink;
            ExecutionMessageSink = executionMessageSink;
            ExecutionOptions = executionOptions;
            TestCaseOrderer = new DefaultTestCaseOrderer();
            TestCollectionOrderer = new DefaultTestCollectionOrderer();
            Aggregator = new ExceptionAggregator();
        }

        /// <summary>
        /// Gets or sets the exception aggregator used to run code and collect exceptions.
        /// </summary>
        protected ExceptionAggregator Aggregator { get; set; }

        /// <summary>
        /// Gets or sets the user's requested execution options.
        /// </summary>
        protected ITestFrameworkExecutionOptions ExecutionOptions { get; set; }

        /// <summary>
        /// Gets or sets the message sink to report diagnostic messages to.
        /// </summary>
        protected IMessageSink DiagnosticMessageSink { get; set; }

        /// <summary>
        /// Gets or sets the message sink to report run status to.
        /// </summary>
        protected IMessageSink ExecutionMessageSink { get; set; }

        /// <summary>
        /// Gets or sets the assembly that contains the tests to be run.
        /// </summary>
        protected ITestAssembly TestAssembly { get; set; }

        /// <summary>
        /// Gets or sets the test case orderer that will be used to decide how to order the tests.
        /// </summary>
        protected ITestCaseOrderer TestCaseOrderer { get; set; }

        /// <summary>
        /// Gets or sets the test collection orderer that will be used to decide how to order the test collections.
        /// </summary>
        protected ITestCollectionOrderer TestCollectionOrderer { get; set; }

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
        protected virtual string GetTestFrameworkEnvironment()
        {
            return String.Format("{0}-bit .NET {1}", IntPtr.Size * 8, GetVersion());
        }

        static string GetVersion()
        {
#if WINDOWS_PHONE_APP || WINDOWS_PHONE || ASPNETCORE50
            var attr = typeof(object).GetTypeInfo().Assembly.GetCustomAttribute<TargetFrameworkAttribute>();
            return attr == null ? "(unknown version)" : attr.FrameworkDisplayName;
#else
            return Environment.Version.ToString();
#endif
        }

        /// <summary>
        /// This method is called just after <see cref="ITestAssemblyStarting"/> is sent, but before any test collections are run.
        /// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
        /// </summary>
        protected virtual Task AfterTestAssemblyStartingAsync()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// This method is called just before <see cref="ITestAssemblyFinished"/> is sent.
        /// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
        /// </summary>
        protected virtual Task BeforeTestAssemblyFinishedAsync()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Creates the message bus to be used for test execution. By default, it inspects
        /// the options for the <see cref="TestOptionsNames.Execution.SynchronousMessageReporting"/>
        /// flag, and if present, creates a message bus that ensures all messages are delivered
        /// on the same thread.
        /// </summary>
        /// <returns>The message bus.</returns>
        protected virtual IMessageBus CreateMessageBus()
        {
            if (ExecutionOptions.SynchronousMessageReportingOrDefault())
                return new SynchronousMessageBus(ExecutionMessageSink);

            return new MessageBus(ExecutionMessageSink);
        }

        /// <summary>
        /// Orders the test collections using the <see cref="TestCollectionOrderer"/>.
        /// </summary>
        /// <returns>Test collections (and the associated test cases) in run order</returns>
        protected List<Tuple<ITestCollection, List<TTestCase>>> OrderTestCases()
        {
            var testCasesByCollection =
                TestCases.GroupBy(tc => tc.TestMethod.TestClass.TestCollection, TestCollectionComparer.Instance)
                         .ToDictionary(collectionGroup => collectionGroup.Key, collectionGroup => collectionGroup.ToList());

            var orderedTestCollections = TestCollectionOrderer.OrderTestCollections(testCasesByCollection.Keys);

            return orderedTestCollections.Select(collection => Tuple.Create(collection, testCasesByCollection[collection]))
                                         .ToList();
        }

        /// <summary>
        /// Runs the tests in the test assembly.
        /// </summary>
        /// <returns>Returns summary information about the tests that were run.</returns>
        public async Task<RunSummary> RunAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var totalSummary = new RunSummary();
#if !WINDOWS_PHONE_APP && !WINDOWS_PHONE && !ASPNET50 && !ASPNETCORE50
            var currentDirectory = Directory.GetCurrentDirectory();
#endif
            var testFrameworkEnvironment = GetTestFrameworkEnvironment();
            var testFrameworkDisplayName = GetTestFrameworkDisplayName();

            using (var messageBus = CreateMessageBus())
            {
#if !WINDOWS_PHONE_APP && !WINDOWS_PHONE && !ASPNET50 && !ASPNETCORE50
                Directory.SetCurrentDirectory(Path.GetDirectoryName(TestAssembly.Assembly.AssemblyPath));
#endif

                if (messageBus.QueueMessage(new TestAssemblyStarting(TestCases.Cast<ITestCase>(), TestAssembly, DateTime.Now, testFrameworkEnvironment, testFrameworkDisplayName)))
                {
                    try
                    {
                        await AfterTestAssemblyStartingAsync();

                        var masterStopwatch = Stopwatch.StartNew();
                        totalSummary = await RunTestCollectionsAsync(messageBus, cancellationTokenSource);
                        // Want clock time, not aggregated run time
                        totalSummary.Time = (decimal)masterStopwatch.Elapsed.TotalSeconds;

                        Aggregator.Clear();
                        await BeforeTestAssemblyFinishedAsync();

                        if (Aggregator.HasExceptions)
                            messageBus.QueueMessage(new TestAssemblyCleanupFailure(TestCases.Cast<ITestCase>(), TestAssembly, Aggregator.ToException()));
                    }
                    finally
                    {
                        messageBus.QueueMessage(new TestAssemblyFinished(TestCases.Cast<ITestCase>(), TestAssembly, totalSummary.Time, totalSummary.Total, totalSummary.Failed, totalSummary.Skipped));
#if !WINDOWS_PHONE_APP && !WINDOWS_PHONE && !ASPNET50 && !ASPNETCORE50
                        Directory.SetCurrentDirectory(currentDirectory);
#endif
                    }
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

            foreach (var collection in OrderTestCases())
            {
                summary.Aggregate(await RunTestCollectionAsync(messageBus, collection.Item1, collection.Item2, cancellationTokenSource));
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
