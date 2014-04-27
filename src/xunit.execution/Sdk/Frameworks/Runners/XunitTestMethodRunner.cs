using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class XunitTestMethodRunner : TestMethodRunner<IXunitTestCase>
    {
        private readonly ExceptionAggregator aggregator;
        private readonly object[] constructorArguments;

        public XunitTestMethodRunner(IMessageBus messageBus,
                                     ITestCollection testCollection,
                                     IReflectionTypeInfo testClass,
                                     IReflectionMethodInfo testMethod,
                                     IEnumerable<IXunitTestCase> testCases,
                                     CancellationTokenSource cancellationTokenSource,
                                     ExceptionAggregator aggregator,
                                     object[] constructorArguments)
            : base(messageBus, testCollection, testClass, testMethod, testCases, cancellationTokenSource)
        {
            this.constructorArguments = constructorArguments;
            this.aggregator = aggregator;
        }

        protected override Task<RunSummary> RunTestCaseAsync(IXunitTestCase testCase)
        {
            return testCase.RunAsync(MessageBus, constructorArguments, aggregator, CancellationTokenSource);
        }
    }
}
