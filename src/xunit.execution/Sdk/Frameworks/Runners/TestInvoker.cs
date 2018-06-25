using System;
using System.Linq;
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
        static MethodInfo startAsTaskOpenGenericMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestInvoker{TTestCase}"/> class.
        /// </summary>
        /// <param name="test">The test that this invocation belongs to.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="testClass">The test class that the test method belongs to.</param>
        /// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
        /// <param name="testMethod">The test method that will be invoked.</param>
        /// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        protected TestInvoker(ITest test,
                              IMessageBus messageBus,
                              Type testClass,
                              object[] constructorArguments,
                              MethodInfo testMethod,
                              object[] testMethodArguments,
                              ExceptionAggregator aggregator,
                              CancellationTokenSource cancellationTokenSource)
        {
            Guard.ArgumentNotNull("test", test);
            Guard.ArgumentValid("test", "test.TestCase must implement " + typeof(TTestCase).FullName, test.TestCase is TTestCase);

            Test = test;
            MessageBus = messageBus;
            TestClass = testClass;
            ConstructorArguments = constructorArguments;
            TestMethod = testMethod;
            TestMethodArguments = testMethodArguments;
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
        /// Gets or sets the constructor arguments used to construct the test class.
        /// </summary>
        protected object[] ConstructorArguments { get; set; }

        /// <summary>
        /// Gets the display name of the invoked test.
        /// </summary>
        protected string DisplayName { get { return Test.DisplayName; } }

        /// <summary>
        /// Gets or sets the message bus to report run status to.
        /// </summary>
        protected IMessageBus MessageBus { get; set; }

        /// <summary>
        /// Gets or sets the test to be run.
        /// </summary>
        protected ITest Test { get; set; }

        /// <summary>
        /// Gets the test case to be run.
        /// </summary>
        protected TTestCase TestCase { get { return (TTestCase)Test.TestCase; } }

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
        protected ExecutionTimer Timer { get; set; } = new ExecutionTimer();

        /// <summary>
        /// Creates the test class, unless the test method is static or there have already been errors. Note that
        /// this method times the creation of the test class (using <see cref="Timer"/>). It is also responsible for
        /// sending the <see cref="ITestClassConstructionStarting"/>and <see cref="ITestClassConstructionFinished"/>
        /// messages, so if you override this method without calling the base, you are responsible for all of this behavior.
        /// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
        /// </summary>
        /// <returns>The class instance, if appropriate; <c>null</c>, otherwise</returns>
        protected virtual object CreateTestClass()
        {
            object testClass = null;

            if (!TestMethod.IsStatic && !Aggregator.HasExceptions)
                testClass = Test.CreateTestClass(TestClass, ConstructorArguments, MessageBus, Timer, CancellationTokenSource);

            return testClass;
        }

        /// <summary>
        /// This method is called just after the test method has finished executing.
        /// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
        /// </summary>
        protected virtual Task AfterTestMethodInvokedAsync()
            => CommonTasks.Completed;

        /// <summary>
        /// This method is called just before the test method is invoked.
        /// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
        /// </summary>
        protected virtual Task BeforeTestMethodInvokedAsync()
            => CommonTasks.Completed;

        /// <summary>
        /// This method calls the test method via reflection. This is an available override point
        /// if you need to do some other form of invocation of the actual test method.
        /// </summary>
        /// <param name="testClassInstance">The instance of the test class</param>
        /// <returns>The return value from the test method invocation</returns>
        protected virtual object CallTestMethod(object testClassInstance)
            => TestMethod.Invoke(testClassInstance, TestMethodArguments);

        /// <summary>
        /// Given an object, will determine if it is an instance of <see cref="Task"/> (in which case, it is
        /// directly returned), or an instance of <see cref="T:Microsoft.FSharp.Control.FSharpAsync`1"/>
        /// (in which case it is converted), or neither (in which case <c>null</c> is returned).
        /// </summary>
        /// <param name="obj">The object to convert</param>
        public static Task GetTaskFromResult(object obj)
        {
            if (obj == null)
                return null;

            var task = obj as Task;
            if (task != null)
                return task;

            var type = obj.GetType();
            if (type.IsGenericType() && type.GetGenericTypeDefinition().FullName == "Microsoft.FSharp.Control.FSharpAsync`1")
            {
                if (startAsTaskOpenGenericMethod == null)
                    startAsTaskOpenGenericMethod = type.GetAssembly().GetType("Microsoft.FSharp.Control.FSharpAsync")
                                                                     .GetRuntimeMethods()
                                                                     .FirstOrDefault(m => m.Name == "StartAsTask");

                return startAsTaskOpenGenericMethod.MakeGenericMethod(type.GetGenericArguments()[0])
                                                   .Invoke(null, new[] { obj, null, null }) as Task;
            }

            return null;
        }

        /// <summary>
        /// Creates the test class (if necessary), and invokes the test method.
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

                    try
                    {
                        var asyncLifetime = testClassInstance as IAsyncLifetime;
                        if (asyncLifetime != null)
                            await asyncLifetime.InitializeAsync();

                        if (!CancellationTokenSource.IsCancellationRequested)
                        {
                            await BeforeTestMethodInvokedAsync();

                            if (!CancellationTokenSource.IsCancellationRequested && !Aggregator.HasExceptions)
                                await InvokeTestMethodAsync(testClassInstance);

                            await AfterTestMethodInvokedAsync();
                        }

                        if (asyncLifetime != null)
                            await Aggregator.RunAsync(asyncLifetime.DisposeAsync);
                    }
                    finally
                    {
                        Aggregator.Run(() => Test.DisposeTestClass(testClassInstance, MessageBus, Timer, CancellationTokenSource));
                    }
                }

                return Timer.Total;
            });
        }

        /// <summary>
        /// Invokes the test method on the given test class instance. This method sets up support for "async void"
        /// test methods, ensures that the test method has the correct number of arguments, then calls <see cref="CallTestMethod"/>
        /// to do the actual method invocation. It ensure that any async test method is fully completed before returning, and
        /// returns the measured clock time that the invocation took.
        /// </summary>
        /// <param name="testClassInstance">The test class instance</param>
        /// <returns>Returns the time taken to invoke the test method</returns>
        protected virtual async Task<decimal> InvokeTestMethodAsync(object testClassInstance)
        {
            var oldSyncContext = SynchronizationContext.Current;

            try
            {
                var asyncSyncContext = new AsyncTestSyncContext(oldSyncContext);
                SetSynchronizationContext(asyncSyncContext);

                await Aggregator.RunAsync(
                    () => Timer.AggregateAsync(
                        async () =>
                        {
                            var parameterCount = TestMethod.GetParameters().Length;
                            var valueCount = TestMethodArguments == null ? 0 : TestMethodArguments.Length;
                            if (parameterCount != valueCount)
                            {
                                Aggregator.Add(
                                    new InvalidOperationException(
                                        $"The test method expected {parameterCount} parameter value{(parameterCount == 1 ? "" : "s")}, but {valueCount} parameter value{(valueCount == 1 ? "" : "s")} {(valueCount == 1 ? "was" : "were")} provided."
                                    )
                                );
                            }
                            else
                            {
                                var result = CallTestMethod(testClassInstance);
                                var task = GetTaskFromResult(result);
                                if (task != null)
                                {
                                    if (task.Status == TaskStatus.Created)
                                        throw new InvalidOperationException("Test method returned a non-started Task (tasks must be started before being returned)");

                                    await task;
                                }
                                else
                                {
                                    var ex = await asyncSyncContext.WaitForCompletionAsync();
                                    if (ex != null)
                                        Aggregator.Add(ex);
                                }
                            }
                        }
                    )
                );
            }
            finally
            {
                SetSynchronizationContext(oldSyncContext);
            }

            return Timer.Total;
        }

        [SecuritySafeCritical]
        static void SetSynchronizationContext(SynchronizationContext context)
            => SynchronizationContext.SetSynchronizationContext(context);
    }
}
