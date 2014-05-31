using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A base class that provides default behavior when running tests in a test method.
    /// </summary>
    /// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
    /// derive from <see cref="ITestCase"/>.</typeparam>
    public abstract class TestMethodRunner<TTestCase>
            where TTestCase : ITestCase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethodRunner{TTestCase}"/> class.
        /// </summary>
        /// <param name="testCollection">The test collection that contains the test class.</param>
        /// <param name="testClass">The test class that contains the test method.</param>
        /// <param name="testMethod">The test method that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        public TestMethodRunner(ITestCollection testCollection,
                                IReflectionTypeInfo testClass,
                                IReflectionMethodInfo testMethod,
                                IEnumerable<TTestCase> testCases,
                                IMessageBus messageBus,
                                CancellationTokenSource cancellationTokenSource)
        {
            MessageBus = messageBus;
            TestCollection = testCollection;
            TestClass = testClass;
            TestMethod = testMethod;
            TestCases = testCases;
            CancellationTokenSource = cancellationTokenSource;
        }

        /// <summary>
        /// Gets or sets he task cancellation token source, used to cancel the test run.
        /// </summary>
        protected CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Gets or sets the message bus to report run status to.
        /// </summary>
        protected IMessageBus MessageBus { get; set; }

        /// <summary>
        /// Gets or sets the test cases to be run.
        /// </summary>
        protected IEnumerable<TTestCase> TestCases { get; set; }

        /// <summary>
        /// Gets or sets the test class that contains the test method.
        /// </summary>
        protected IReflectionTypeInfo TestClass { get; set; }

        /// <summary>
        /// Gets or sets the test collection that contains the test class.
        /// </summary>
        protected ITestCollection TestCollection { get; set; }

        /// <summary>
        /// Gets or sets the test method that contains the tests to be run.
        /// </summary>
        protected IReflectionMethodInfo TestMethod { get; set; }

        /// <summary>
        /// This method is called just before <see cref="ITestMethodStarting"/> is sent.
        /// </summary>
        protected virtual void OnTestMethodStarting() { }

        /// <summary>
        /// This method is called just after <see cref="ITestMethodStarting"/> is sent, but before any test cases are run.
        /// </summary>
        protected virtual void OnTestMethodStarted() { }

        /// <summary>
        /// This method is called just before <see cref="ITestMethodFinished"/> is sent.
        /// </summary>
        protected virtual void OnTestMethodFinishing() { }

        /// <summary>
        /// This method is called just after <see cref="ITestMethodFinished"/> is sent.
        /// </summary>
        protected virtual void OnTestMethodFinished() { }

        /// <summary>
        /// Runs the tests in the test method.
        /// </summary>
        /// <returns>Returns summary information about the tests that were run.</returns>
        public async Task<RunSummary> RunAsync()
        {
            OnTestMethodStarting();

            var methodSummary = new RunSummary();

            try
            {
                if (!MessageBus.QueueMessage(new TestMethodStarting(TestCollection, TestClass.Name, TestMethod.Name)))
                    CancellationTokenSource.Cancel();
                else
                {
                    OnTestMethodStarted();

                    methodSummary = await RunTestCasesAsync();
                }
            }
            finally
            {
                OnTestMethodFinishing();

                if (!MessageBus.QueueMessage(new TestMethodFinished(TestCollection, TestClass.Name, TestMethod.Name)))
                    CancellationTokenSource.Cancel();

                OnTestMethodFinished();
            }

            return methodSummary;
        }

        /// <summary>
        /// Runs the list of test cases. By default, it runs the cases in order, synchronously.
        /// </summary>
        /// <returns>Returns summary information about the tests that were run.</returns>
        protected virtual async Task<RunSummary> RunTestCasesAsync()
        {
            var summary = new RunSummary();

            foreach (var testCase in TestCases)
            {
                summary.Aggregate(await RunTestCaseAsync(testCase));
                if (CancellationTokenSource.IsCancellationRequested)
                    break;
            }

            return summary;
        }

        /// <summary>
        /// Override this method to run an individual test case.
        /// </summary>
        /// <param name="testCase">The test case to be run.</param>
        /// <returns>Returns summary information about the test case run.</returns>
        protected abstract Task<RunSummary> RunTestCaseAsync(TTestCase testCase);
    }
}
