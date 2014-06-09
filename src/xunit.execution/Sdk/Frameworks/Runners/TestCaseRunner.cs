using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A base class that provides default behavior when running test cases.
    /// </summary>
    /// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
    /// derive from <see cref="ITestCase"/>.</typeparam>
    public abstract class TestCaseRunner<TTestCase>
        where TTestCase : ITestCase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCaseRunner{TTestCase}"/> class.
        /// </summary>
        /// <param name="testCase">The test case to be run.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        public TestCaseRunner(TTestCase testCase,
                              IMessageBus messageBus,
                              ExceptionAggregator aggregator,
                              CancellationTokenSource cancellationTokenSource)
        {
            TestCase = testCase;
            MessageBus = messageBus;
            Aggregator = aggregator;
            CancellationTokenSource = cancellationTokenSource;
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
        /// Gets or sets the test case to be run.
        /// </summary>
        protected TTestCase TestCase { get; set; }

        /// <summary>
        /// This method is called just before <see cref="ITestCaseStarting"/> is sent.
        /// </summary>
        protected virtual void OnTestCaseStarting() { }

        /// <summary>
        /// This method is called just after <see cref="ITestCaseStarting"/> it sent, but before any test collections are run.
        /// </summary>
        protected virtual void OnTestCaseStarted() { }

        /// <summary>
        /// This method is called just before <see cref="ITestCaseFinished"/> is sent.
        /// </summary>
        protected virtual void OnTestCaseFinishing() { }

        /// <summary>
        /// This method is called just after <see cref="ITestCaseFinished"/> is sent.
        /// </summary>
        protected virtual void OnTestCaseFinished() { }

        /// <summary>
        /// Runs the tests in the test case.
        /// </summary>
        /// <returns>Returns summary information about the tests that were run.</returns>
        public async Task<RunSummary> RunAsync()
        {
            var summary = new RunSummary();

            OnTestCaseStarting();

            if (!MessageBus.QueueMessage(new TestCaseStarting(TestCase)))
                CancellationTokenSource.Cancel();
            else
            {
                OnTestCaseStarted();

                // TODO: Introduce TestCaseFailedHere, only calling RunTestAsync if things are still okay (also harden OnXxx implementations)
                summary = await RunTestAsync();

                OnTestCaseFinishing();
            }

            if (!MessageBus.QueueMessage(new TestCaseFinished(TestCase, summary.Time, summary.Total, summary.Failed, summary.Skipped)))
                CancellationTokenSource.Cancel();

            OnTestCaseFinished();

            return summary;
        }

        /// <summary>
        /// Override this method to run the tests in an individual test method.
        /// </summary>
        /// <returns>Returns summary information about the tests that were run.</returns>
        protected abstract Task<RunSummary> RunTestAsync();
    }
}
