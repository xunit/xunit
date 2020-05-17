using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    /// <summary>
    /// An implementation of <see cref="TestCaseRunner{TTestCase}"/> to support <see cref="ExecutionErrorTestCase"/>.
    /// </summary>
    public class ExecutionErrorTestCaseRunner : TestCaseRunner<ExecutionErrorTestCase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionErrorTestCaseRunner"/> class.
        /// </summary>
        /// <param name="testCase">The test case that the lambda represents.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        public ExecutionErrorTestCaseRunner(ExecutionErrorTestCase testCase,
                                            IMessageBus messageBus,
                                            ExceptionAggregator aggregator,
                                            CancellationTokenSource cancellationTokenSource)
            : base(testCase, messageBus, aggregator, cancellationTokenSource) { }

        /// <inheritdoc/>
        protected override Task<RunSummary> RunTestAsync()
        {
            var test = new XunitTest(TestCase, TestCase.DisplayName);
            var summary = new RunSummary { Total = 1 };

            if (!MessageBus.QueueMessage(new TestStarting(test)))
                CancellationTokenSource.Cancel();
            else
            {
                summary.Failed = 1;

                var testFailed = new TestFailed(test, 0, null,
                                                new[] { typeof(InvalidOperationException).FullName },
                                                new[] { TestCase.ErrorMessage },
                                                new[] { "" },
                                                new[] { -1 });

                if (!MessageBus.QueueMessage(testFailed))
                    CancellationTokenSource.Cancel();

                if (!MessageBus.QueueMessage(new TestFinished(test, 0, null)))
                    CancellationTokenSource.Cancel();
            }

            return Task.FromResult(summary);
        }
    }
}
