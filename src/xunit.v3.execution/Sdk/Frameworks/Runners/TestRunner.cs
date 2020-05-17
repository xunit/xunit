using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A base class that provides default behavior when running a test. This includes support
    /// for skipping tests.
    /// </summary>
    /// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
    /// derive from <see cref="ITestCase"/>.</typeparam>
    public abstract class TestRunner<TTestCase>
        where TTestCase : ITestCase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunner{TTestCase}"/> class.
        /// </summary>
        /// <param name="test">The test that this invocation belongs to.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="testClass">The test class that the test method belongs to.</param>
        /// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
        /// <param name="testMethod">The test method that will be invoked.</param>
        /// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
        /// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        protected TestRunner(ITest test,
                             IMessageBus messageBus,
                             Type testClass,
                             object[] constructorArguments,
                             MethodInfo testMethod,
                             object[] testMethodArguments,
                             string skipReason,
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
            SkipReason = skipReason;
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
        /// Gets or sets the display name of the invoked test.
        /// </summary>
        protected string DisplayName { get { return Test.DisplayName; } }

        /// <summary>
        /// Gets or sets the message bus to report run status to.
        /// </summary>
        protected IMessageBus MessageBus { get; set; }

        /// <summary>
        /// Gets or sets the skip reason for the test, if set.
        /// </summary>
        protected string SkipReason { get; set; }

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
        /// This method is called just after <see cref="ITestStarting"/> is sent, but before the test class is created.
        /// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
        /// </summary>
        protected virtual void AfterTestStarting() { }

        /// <summary>
        /// This method is called just before <see cref="ITestFinished"/> is sent.
        /// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
        /// </summary>
        protected virtual void BeforeTestFinished() { }

        /// <summary>
        /// Runs the test.
        /// </summary>
        /// <returns>Returns summary information about the test that was run.</returns>
        public async Task<RunSummary> RunAsync()
        {
            var runSummary = new RunSummary { Total = 1 };
            var output = string.Empty;

            if (!MessageBus.QueueMessage(new TestStarting(Test)))
                CancellationTokenSource.Cancel();
            else
            {
                AfterTestStarting();

                if (!string.IsNullOrEmpty(SkipReason))
                {
                    runSummary.Skipped++;

                    if (!MessageBus.QueueMessage(new TestSkipped(Test, SkipReason)))
                        CancellationTokenSource.Cancel();
                }
                else
                {
                    var aggregator = new ExceptionAggregator(Aggregator);

                    if (!aggregator.HasExceptions)
                    {
                        var tuple = await aggregator.RunAsync(() => InvokeTestAsync(aggregator));
                        if (tuple != null)
                        {
                            runSummary.Time = tuple.Item1;
                            output = tuple.Item2;
                        }
                    }

                    var exception = aggregator.ToException();
                    TestResultMessage testResult;

                    if (exception == null)
                        testResult = new TestPassed(Test, runSummary.Time, output);
                    else
                    {
                        testResult = new TestFailed(Test, runSummary.Time, output, exception);
                        runSummary.Failed++;
                    }

                    if (!CancellationTokenSource.IsCancellationRequested)
                        if (!MessageBus.QueueMessage(testResult))
                            CancellationTokenSource.Cancel();
                }

                Aggregator.Clear();
                BeforeTestFinished();

                if (Aggregator.HasExceptions)
                    if (!MessageBus.QueueMessage(new TestCleanupFailure(Test, Aggregator.ToException())))
                        CancellationTokenSource.Cancel();

                if (!MessageBus.QueueMessage(new TestFinished(Test, runSummary.Time, output)))
                    CancellationTokenSource.Cancel();
            }

            return runSummary;
        }

        /// <summary>
        /// Override this method to invoke the test.
        /// </summary>
        /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
        /// <returns>Returns a tuple which includes the execution time (in seconds) spent running the
        /// test method, and any output that was returned by the test.</returns>
        protected abstract Task<Tuple<decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator);
    }
}
