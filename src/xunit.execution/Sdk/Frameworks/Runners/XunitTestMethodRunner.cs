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
        private readonly object[] constructorArguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestMethodRunner"/> class.
        /// </summary>
        /// <param name="testMethod">The test method to be run.</param>
        /// <param name="class">The test class that contains the test method.</param>
        /// <param name="method">The test method that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        /// <param name="constructorArguments">The constructor arguments for the test class.</param>
        public XunitTestMethodRunner(ITestMethod testMethod,
                                     IReflectionTypeInfo @class,
                                     IReflectionMethodInfo method,
                                     IEnumerable<IXunitTestCase> testCases,
                                     IMessageBus messageBus,
                                     ExceptionAggregator aggregator,
                                     CancellationTokenSource cancellationTokenSource,
                                     object[] constructorArguments)
            : base(testMethod, @class, method, testCases, messageBus, aggregator, cancellationTokenSource)
        {
            this.constructorArguments = constructorArguments;
        }

        /// <inheritdoc/>
        protected override Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
        {
            return testCase.RunAsync(MessageBus, constructorArguments, Aggregator, CancellationTokenSource);
        }
    }
}
