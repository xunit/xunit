using System.Collections.Generic;
using System.Linq;
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
        /// <param name="testMethod">The test method under test.</param>
        /// <param name="class">The CLR class that contains the test method.</param>
        /// <param name="method">The CLR method that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        protected TestMethodRunner(ITestMethod testMethod,
                                   IReflectionTypeInfo @class,
                                   IReflectionMethodInfo method,
                                   IEnumerable<TTestCase> testCases,
                                   IMessageBus messageBus,
                                   ExceptionAggregator aggregator,
                                   CancellationTokenSource cancellationTokenSource)
        {
            TestMethod = testMethod;
            Class = @class;
            Method = method;
            TestCases = testCases;
            MessageBus = messageBus;
            Aggregator = aggregator;
            CancellationTokenSource = cancellationTokenSource;
        }

        /// <summary>
        /// Gets or sets the exception aggregator used to run code and collect exceptions.
        /// </summary>
        protected ExceptionAggregator Aggregator { get; set; }

        /// <summary>
        /// Gets or sets the task cancellation token source, used to cancel the test run.
        /// </summary>
        protected CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Gets or sets the CLR class that contains the test method.
        /// </summary>
        protected IReflectionTypeInfo Class { get; set; }

        /// <summary>
        /// Gets or sets the message bus to report run status to.
        /// </summary>
        protected IMessageBus MessageBus { get; set; }

        /// <summary>
        /// Gets or sets the CLR method that contains the tests to be run.
        /// </summary>
        protected IReflectionMethodInfo Method { get; set; }

        /// <summary>
        /// Gets or sets the test cases to be run.
        /// </summary>
        protected IEnumerable<TTestCase> TestCases { get; set; }

        /// <summary>
        /// Gets or sets the test method that contains the test cases.
        /// </summary>
        protected ITestMethod TestMethod { get; set; }

        /// <summary>
        /// This method is called just after <see cref="ITestMethodStarting"/> is sent, but before any test cases are run.
        /// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
        /// </summary>
        protected virtual void AfterTestMethodStarting() { }

        /// <summary>
        /// This method is called just before <see cref="ITestMethodFinished"/> is sent.
        /// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
        /// </summary>
        protected virtual void BeforeTestMethodFinished() { }

        /// <summary>
        /// Runs the tests in the test method.
        /// </summary>
        /// <returns>Returns summary information about the tests that were run.</returns>
        public async Task<RunSummary> RunAsync()
        {
            var methodSummary = new RunSummary();

            if (!MessageBus.QueueMessage(new TestMethodStarting(TestCases.Cast<ITestCase>(), TestMethod)))
                CancellationTokenSource.Cancel();
            else
            {
                try
                {
                    AfterTestMethodStarting();
                    methodSummary = await RunTestCasesAsync();

                    Aggregator.Clear();
                    BeforeTestMethodFinished();

                    if (Aggregator.HasExceptions)
                        if (!MessageBus.QueueMessage(new TestMethodCleanupFailure(TestCases.Cast<ITestCase>(), TestMethod, Aggregator.ToException())))
                            CancellationTokenSource.Cancel();
                }
                finally
                {
                    if (!MessageBus.QueueMessage(new TestMethodFinished(TestCases.Cast<ITestCase>(), TestMethod, methodSummary.Time, methodSummary.Total, methodSummary.Failed, methodSummary.Skipped)))
                        CancellationTokenSource.Cancel();

                }
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
