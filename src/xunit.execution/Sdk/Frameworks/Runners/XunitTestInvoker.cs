using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The test invoker for xUnit.net v2 tests.
    /// </summary>
    public class XunitTestInvoker : TestInvoker<IXunitTestCase>
    {
        readonly IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes;
        readonly Stack<BeforeAfterTestAttribute> beforeAfterAttributesRun = new Stack<BeforeAfterTestAttribute>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestInvoker"/> class.
        /// </summary>
        /// <param name="test">The test that this invocation belongs to.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="testClass">The test class that the test method belongs to.</param>
        /// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
        /// <param name="testMethod">The test method that will be invoked.</param>
        /// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
        /// <param name="beforeAfterAttributes">The list of <see cref="BeforeAfterTestAttribute"/>s for this test invocation.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        public XunitTestInvoker(ITest test,
                                IMessageBus messageBus,
                                Type testClass,
                                object[] constructorArguments,
                                MethodInfo testMethod,
                                object[] testMethodArguments,
                                IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
                                ExceptionAggregator aggregator,
                                CancellationTokenSource cancellationTokenSource)
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, aggregator, cancellationTokenSource)
        {
            this.beforeAfterAttributes = beforeAfterAttributes;
        }

        /// <summary>
        /// Gets the list of <see cref="BeforeAfterTestAttribute"/>s for this test invocation.
        /// </summary>
        protected IReadOnlyList<BeforeAfterTestAttribute> BeforeAfterAttributes
            => beforeAfterAttributes;

        /// <inheritdoc/>
        protected override Task BeforeTestMethodInvokedAsync()
        {
            foreach (var beforeAfterAttribute in beforeAfterAttributes)
            {
                var attributeName = beforeAfterAttribute.GetType().Name;
                if (!MessageBus.QueueMessage(new BeforeTestStarting(Test, attributeName)))
                    CancellationTokenSource.Cancel();
                else
                {
                    try
                    {
                        Timer.Aggregate(() => beforeAfterAttribute.Before(TestMethod));
                        beforeAfterAttributesRun.Push(beforeAfterAttribute);
                    }
                    catch (Exception ex)
                    {
                        Aggregator.Add(ex);
                        break;
                    }
                    finally
                    {
                        if (!MessageBus.QueueMessage(new BeforeTestFinished(Test, attributeName)))
                            CancellationTokenSource.Cancel();
                    }
                }

                if (CancellationTokenSource.IsCancellationRequested)
                    break;
            }

            return CommonTasks.Completed;
        }

        /// <inheritdoc/>
        protected override Task AfterTestMethodInvokedAsync()
        {
            foreach (var beforeAfterAttribute in beforeAfterAttributesRun)
            {
                var attributeName = beforeAfterAttribute.GetType().Name;
                if (!MessageBus.QueueMessage(new AfterTestStarting(Test, attributeName)))
                    CancellationTokenSource.Cancel();

                Aggregator.Run(() => Timer.Aggregate(() => beforeAfterAttribute.After(TestMethod)));

                if (!MessageBus.QueueMessage(new AfterTestFinished(Test, attributeName)))
                    CancellationTokenSource.Cancel();
            }

            return CommonTasks.Completed;
        }

        /// <inheritdoc/>
        protected override Task<decimal> InvokeTestMethodAsync(object testClassInstance)
        {
            if (TestCase.InitializationException != null)
            {
                var tcs = new TaskCompletionSource<decimal>();
                tcs.SetException(TestCase.InitializationException);
                return tcs.Task;
            }

            return TestCase.Timeout > 0
                ? InvokeTimeoutTestMethodAsync(testClassInstance)
                : base.InvokeTestMethodAsync(testClassInstance);
        }

        async Task<decimal> InvokeTimeoutTestMethodAsync(object testClassInstance)
        {
            var baseTask = base.InvokeTestMethodAsync(testClassInstance);
            var resultTask = await Task.WhenAny(baseTask, Task.Delay(TestCase.Timeout));

            if (resultTask != baseTask)
                throw new TestTimeoutException(TestCase.Timeout);

            return baseTask.Result;
        }
    }
}
