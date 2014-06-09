using System;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A base class that provides default behavior to invoke a test method. This includes
    /// support for async test methods (both "async Task" and "async void") as well as
    /// creation and disposal of the test class.
    /// </summary>
    /// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
    /// derive from <see cref="ITestCase"/>.</typeparam>
    public abstract class TestInvoker<TTestCase>
        where TTestCase : ITestCase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestInvoker{TTestCase}"/> class.
        /// </summary>
        /// <param name="testCase">The test case that this invocation belongs to.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="testClass">The test class that the test method belongs to.</param>
        /// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
        /// <param name="testMethod">The test method that will be invoked.</param>
        /// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
        /// <param name="displayName">The display name for this test invocation.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        public TestInvoker(TTestCase testCase,
                           IMessageBus messageBus,
                           Type testClass,
                           object[] constructorArguments,
                           MethodInfo testMethod,
                           object[] testMethodArguments,
                           string displayName,
                           ExceptionAggregator aggregator,
                           CancellationTokenSource cancellationTokenSource)
        {
            TestCase = testCase;
            MessageBus = messageBus;
            TestClass = testClass;
            ConstructorArguments = constructorArguments;
            TestMethod = testMethod;
            TestMethodArguments = testMethodArguments;
            DisplayName = displayName;
            Aggregator = aggregator;
            CancellationTokenSource = cancellationTokenSource;

            Timer = new ExecutionTimer();
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
        /// Gets or sets the constructor arguments used to construct the test class.
        /// </summary>
        protected object[] ConstructorArguments { get; set; }

        /// <summary>
        /// Gets or sets the display name of the invoked test.
        /// </summary>
        protected string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the message bus to report run status to.
        /// </summary>
        protected IMessageBus MessageBus { get; set; }

        /// <summary>
        /// Gets or sets the test case to be run.
        /// </summary>
        protected TTestCase TestCase { get; set; }

        /// <summary>
        /// Gets or sets the runtime type of the class that contains the test method.
        /// </summary>
        protected Type TestClass { get; set; }

        /// <summary>
        /// Gets or sets the runtime method of the method that contains the test.
        /// </summary>
        protected MethodInfo TestMethod { get; set; }

        /// <summary>
        /// Gets or sets the arguments to pass to the test method when it's being invoked.
        /// </summary>
        protected object[] TestMethodArguments { get; set; }

        /// <summary>
        /// Gets or sets the object which measures execution time.
        /// </summary>
        protected ExecutionTimer Timer { get; set; }

        object CreateTestClass()
        {
            object testClass = null;

            if (!TestMethod.IsStatic)
            {
                if (!MessageBus.QueueMessage(new TestClassConstructionStarting(TestCase, DisplayName)))
                    CancellationTokenSource.Cancel();

                try
                {
                    if (!CancellationTokenSource.IsCancellationRequested)
                        Timer.Aggregate(() => testClass = Activator.CreateInstance(TestClass, ConstructorArguments));
                }
                finally
                {
                    if (!MessageBus.QueueMessage(new TestClassConstructionFinished(TestCase, DisplayName)))
                        CancellationTokenSource.Cancel();
                }
            }

            return testClass;
        }

        void DisposeTestClass(object testClass)
        {
            var disposable = testClass as IDisposable;
            if (disposable == null)
                return;

            if (!MessageBus.QueueMessage(new TestClassDisposeStarting(TestCase, DisplayName)))
                CancellationTokenSource.Cancel();

            try
            {
                Timer.Aggregate(disposable.Dispose);
            }
            finally
            {
                if (!MessageBus.QueueMessage(new TestClassDisposeFinished(TestCase, DisplayName)))
                    CancellationTokenSource.Cancel();
            }
        }

        /// <summary>
        /// This method is called just before the test method is invoked.
        /// </summary>
        protected virtual void OnTestExecuting() { }

        /// <summary>
        /// This method is called just after the test method has finished executing.
        /// </summary>
        protected virtual void OnTestExecuted() { }

        /// <summary>
        /// Invokes the test method.
        /// </summary>
        /// <returns>Returns the time (in seconds) spent creating the test class, running
        /// the test, and disposing of the test class.</returns>
        public Task<decimal> RunAsync()
        {
            return Aggregator.RunAsync(async () =>
            {
                if (!CancellationTokenSource.IsCancellationRequested)
                {
                    var testClassInstance = CreateTestClass();
                    // CreateTestClass() will throw, so no need to check the aggregator

                    if (!CancellationTokenSource.IsCancellationRequested)
                    {
                        OnTestExecuting();

                        if (!Aggregator.HasExceptions)
                            await InvokeTestMethodAsync(testClassInstance);

                        OnTestExecuted();
                    }

                    Aggregator.Run(() => DisposeTestClass(testClassInstance));
                }

                return Timer.Total;
            });
        }

        /// <summary>
        /// For unit testing purposes only.
        /// </summary>
        protected virtual async Task InvokeTestMethodAsync(object testClassInstance)
        {
            var oldSyncContext = SynchronizationContext.Current;

            try
            {
                var asyncSyncContext = new AsyncTestSyncContext();
                SetSynchronizationContext(asyncSyncContext);

                await Aggregator.RunAsync(
                    () => Timer.AggregateAsync(
                        async () =>
                        {
                            var result = TestMethod.Invoke(testClassInstance, TestMethodArguments);
                            var task = result as Task;
                            if (task != null)
                                await task;
                            else
                            {
                                var ex = await asyncSyncContext.WaitForCompletionAsync();
                                if (ex != null)
                                    Aggregator.Add(ex);
                            }
                        }
                    )
                );
            }
            finally
            {
                SetSynchronizationContext(oldSyncContext);
            }
        }

        [SecuritySafeCritical]
        static void SetSynchronizationContext(SynchronizationContext context)
        {
            SynchronizationContext.SetSynchronizationContext(context);
        }
    }
}
