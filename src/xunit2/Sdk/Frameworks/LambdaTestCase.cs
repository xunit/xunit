using System;
using System.Runtime.Remoting;
using System.Security;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A simple implementation of <see cref="XunitTestCase"/> wherein the running of the
    /// test case can be represented by an <see cref="Action"/>. Useful for emitting test
    /// cases which later evaluate to error messages (since throwing error messages during
    /// discovery is often the wrong thing to do). See <see cref="TheoryDiscoverer"/> for
    /// a use of this test case to emit an error message when a theory method is found
    /// that has no test data associated with it.
    /// </summary>
    public class LambdaTestCase : XunitTestCase
    {
        readonly Action lambda;

        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaTestCase"/> class.
        /// </summary>
        /// <param name="testCollection">The test collection this test case belongs to.</param>
        /// <param name="assembly">The test assembly.</param>
        /// <param name="testClass">The test class.</param>
        /// <param name="testMethod">The test method.</param>
        /// <param name="factAttribute">The instance of <see cref="FactAttribute"/>.</param>
        /// <param name="lambda">The code to run for the test.</param>
        public LambdaTestCase(ITestCollection testCollection, IAssemblyInfo assembly, ITypeInfo testClass, IMethodInfo testMethod, IAttributeInfo factAttribute, Action lambda)
            : base(testCollection, assembly, testClass, testMethod, factAttribute)
        {
            this.lambda = lambda;
        }

        /// <inheritdoc/>
        protected override void RunTests(IMessageSink messageSink, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            if (!OnMessage(messageSink, new TestStarting(this, DisplayName)))
                cancellationTokenSource.Cancel();
            else
            {
                try
                {
                    lambda();

                    if (!OnMessage(messageSink, new TestPassed(this, DisplayName, 0, null)))
                        cancellationTokenSource.Cancel();
                }
                catch (Exception ex)
                {
                    if (!OnMessage(messageSink, new TestFailed(this, DisplayName, 0, null, ex)))
                        cancellationTokenSource.Cancel();
                }
            }

            if (!OnMessage(messageSink, new TestFinished(this, DisplayName, 0, null)))
                cancellationTokenSource.Cancel();
        }
    }
}