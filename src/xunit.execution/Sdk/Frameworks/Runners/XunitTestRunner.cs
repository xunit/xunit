using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    /// <summary>
    /// The test runner for xUnit.net v2 tests.
    /// </summary>
    public class XunitTestRunner : TestRunner<XunitTestCase>
    {
        readonly IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestRunner"/> class.
        /// </summary>
        /// <param name="testCase">The test case that this invocation belongs to.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="testClass">The test class that the test method belongs to.</param>
        /// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
        /// <param name="testMethod">The test method that will be invoked.</param>
        /// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
        /// <param name="displayName">The display name for this test invocation.</param>
        /// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
        /// <param name="beforeAfterAttributes">The list of <see cref="BeforeAfterTestAttribute"/>s for this test.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        public XunitTestRunner(XunitTestCase testCase,
                               IMessageBus messageBus,
                               Type testClass,
                               object[] constructorArguments,
                               MethodInfo testMethod,
                               object[] testMethodArguments,
                               string displayName,
                               string skipReason,
                               IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
                               ExceptionAggregator aggregator,
                               CancellationTokenSource cancellationTokenSource)
            : base(testCase, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, displayName, skipReason, aggregator, cancellationTokenSource)
        {
            this.beforeAfterAttributes = beforeAfterAttributes;
        }

        /// <inheritdoc/>
        protected override Task<decimal> InvokeTestAsync(ExceptionAggregator aggregator)
        {
            return new XunitTestInvoker(TestCase, MessageBus, TestClass, ConstructorArguments, TestMethod, TestMethodArguments, beforeAfterAttributes, DisplayName, aggregator, CancellationTokenSource).RunAsync();
        }
    }
}
