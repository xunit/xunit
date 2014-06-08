using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A base class that provides default behavior when running tests in a test collection. It groups the tests
    /// by test class, and then runs the individual test classes.
    /// </summary>
    /// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
    /// derive from <see cref="ITestCase"/>.</typeparam>
    public abstract class TestCollectionRunner<TTestCase>
        where TTestCase : ITestCase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCollectionRunner{TTestCase}"/> class.
        /// </summary>
        /// <param name="testCollection">The test collection that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the test.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        public TestCollectionRunner(ITestCollection testCollection,
                                    IEnumerable<TTestCase> testCases,
                                    IMessageBus messageBus,
                                    ITestCaseOrderer testCaseOrderer,
                                    CancellationTokenSource cancellationTokenSource)
        {
            TestCollection = testCollection;
            TestCases = testCases;
            MessageBus = messageBus;
            TestCaseOrderer = testCaseOrderer;
            CancellationTokenSource = cancellationTokenSource;
            Aggregator = new ExceptionAggregator();
        }

        /// <summary>
        /// Gets or sets the exception aggregator used to run code and collection exceptions.
        /// </summary>
        protected ExceptionAggregator Aggregator { get; set; }

        /// <summary>
        /// Gets or sets the task cancellation token source, used to cancel the test run.
        /// </summary>
        protected CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Gets or sets the message bus to report run status to.
        /// </summary>
        protected IMessageBus MessageBus { get; set; }

        /// <summary>
        /// Gets or sets the test case orderer that will be used to decide how to order the test.
        /// </summary>
        protected ITestCaseOrderer TestCaseOrderer { get; set; }

        /// <summary>
        /// Gets or sets the test cases to be run.
        /// </summary>
        protected IEnumerable<TTestCase> TestCases { get; set; }

        /// <summary>
        /// Gets or sets the test collection that contains the tests to be run.
        /// </summary>
        protected ITestCollection TestCollection { get; set; }

        /// <summary>
        /// This method is called just before <see cref="ITestCollectionStarting"/> is sent.
        /// </summary>
        protected virtual void OnTestCollectionStarting() { }

        /// <summary>
        /// This method is called just after <see cref="ITestCollectionStarting"/> is sent, but before any test classes are run.
        /// </summary>
        protected virtual void OnTestCollectionStarted() { }

        /// <summary>
        /// This method is called just before <see cref="ITestCollectionFinished"/> is sent.
        /// </summary>
        protected virtual void OnTestCollectionFinishing() { }

        /// <summary>
        /// This method is called just after <see cref="ITestCollectionFinished"/> is sent.
        /// </summary>
        protected virtual void OnTestCollectionFinished() { }

        /// <summary>
        /// Runs the tests in the test collection.
        /// </summary>
        /// <returns>Returns summary information about the tests that were run.</returns>
        public async Task<RunSummary> RunAsync()
        {
            OnTestCollectionStarting();

            var collectionSummary = new RunSummary();

            try
            {
                if (!MessageBus.QueueMessage(new TestCollectionStarting(TestCollection)))
                    CancellationTokenSource.Cancel();
                else
                {
                    OnTestCollectionStarted();

                    collectionSummary = await RunTestClassesAsync();
                }
            }
            finally
            {
                OnTestCollectionFinishing();

                if (!MessageBus.QueueMessage(new TestCollectionFinished(TestCollection, collectionSummary.Time, collectionSummary.Total, collectionSummary.Failed, collectionSummary.Skipped)))
                    CancellationTokenSource.Cancel();

                OnTestCollectionFinished();
            }

            return collectionSummary;
        }

        /// <summary>
        /// Runs the list of test classes. By default, groups the tests by class and runs them synchronously.
        /// </summary>
        /// <returns>Returns summary information about the tests that were run.</returns>
        protected virtual async Task<RunSummary> RunTestClassesAsync()
        {
            var summary = new RunSummary();

            foreach (var testCasesByClass in TestCases.GroupBy(tc => tc.Class))
            {
                summary.Aggregate(await RunTestClassAsync((IReflectionTypeInfo)testCasesByClass.Key, testCasesByClass));
                if (CancellationTokenSource.IsCancellationRequested)
                    break;
            }

            return summary;
        }

        /// <summary>
        /// Override this method to run the tests in an individual test class.
        /// </summary>
        /// <param name="testClass">The test class that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <returns>Returns summary information about the tests that were run.</returns>
        protected abstract Task<RunSummary> RunTestClassAsync(IReflectionTypeInfo testClass, IEnumerable<TTestCase> testCases);
    }
}
