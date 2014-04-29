using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The test method runner for xUnit.net v2 tests.
    /// </summary>
    public class XunitTestMethodRunner : TestMethodRunner<IXunitTestCase>
    {
        private readonly ExceptionAggregator aggregator;
        private readonly object[] constructorArguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestMethodRunner"/> class.
        /// </summary>
        /// <param name="testCollection">The test collection that contains the test class.</param>
        /// <param name="testClass">The test class that contains the test method.</param>
        /// <param name="testMethod">The test method that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
        /// <param name="constructorArguments">The constructor arguments for the test class.</param>
        public XunitTestMethodRunner(ITestCollection testCollection,
                                     IReflectionTypeInfo testClass,
                                     IReflectionMethodInfo testMethod,
                                     IEnumerable<IXunitTestCase> testCases,
                                     IMessageBus messageBus,
                                     CancellationTokenSource cancellationTokenSource,
                                     ExceptionAggregator aggregator,
                                     object[] constructorArguments)
            : base(testCollection, testClass, testMethod, testCases, messageBus, cancellationTokenSource)
        {
            this.constructorArguments = constructorArguments;
            this.aggregator = aggregator;
        }

        /// <inheritdoc/>
        protected override Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
        {
            return testCase.RunAsync(MessageBus, constructorArguments, aggregator, CancellationTokenSource);
        }
    }
}
