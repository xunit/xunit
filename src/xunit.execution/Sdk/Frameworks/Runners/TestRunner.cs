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
        /// Initalizes a new instance of the <see cref="TestRunner{TTestCase}"/> class.
        /// </summary>
        /// <param name="testCase">The test case that this invocation belongs to.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="testClass">The test class that the test method belongs to.</param>
        /// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
        /// <param name="testMethod">The test method that will be invoked.</param>
        /// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
        /// <param name="displayName">The display name for this test invocation.</param>
        /// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        public TestRunner(TTestCase testCase,
                          IMessageBus messageBus,
                          Type testClass,
                          object[] constructorArguments,
                          MethodInfo testMethod,
                          object[] testMethodArguments,
                          string displayName,
                          string skipReason,
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
            SkipReason = skipReason;
            Aggregator = aggregator;
            CancellationTokenSource = cancellationTokenSource;
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
        /// Gets or sets the skip reason for the test, if set.
        /// </summary>
        protected string SkipReason { get; set; }

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
        /// This method is called just before <see cref="ITestStarting"/> is sent.
        /// </summary>
        protected virtual void OnTestStarting() { }

        /// <summary>
        /// This method is called just after <see cref="ITestStarting"/> is sent, but before the test class is created.
        /// </summary>
        protected virtual void OnTestStarted() { }

        /// <summary>
        /// This method is called just before <see cref="ITestFinished"/> is sent.
        /// </summary>
        protected virtual void OnTestFinishing() { }

        /// <summary>
        /// This method is called just after <see cref="ITestFinished"/> is sent.
        /// </summary>
        protected virtual void OnTestFinished() { }

        /// <summary>
        /// Runs the test.
        /// </summary>
        /// <returns>Returns summary information about the test that was run.</returns>
        public async Task<RunSummary> RunAsync()
        {
            var runSummary = new RunSummary { Total = 1 };
            var aggregator = new ExceptionAggregator(Aggregator);
            var output = String.Empty;  // TODO: Add output facilities for v2

            OnTestStarting();

            if (!MessageBus.QueueMessage(new TestStarting(TestCase, DisplayName)))
                CancellationTokenSource.Cancel();
            else
            {
                OnTestStarted();

                if (!String.IsNullOrEmpty(SkipReason))
                {
                    runSummary.Skipped++;

                    if (!MessageBus.QueueMessage(new TestSkipped(TestCase, DisplayName, SkipReason)))
                        CancellationTokenSource.Cancel();
                }
                else
                {
                    if (!aggregator.HasExceptions)
                        runSummary.Time = await aggregator.RunAsync(() => InvokeTestAsync(aggregator));

                    var exception = aggregator.ToException();
                    TestResultMessage testResult;

                    if (exception == null)
                        testResult = new TestPassed(TestCase, DisplayName, runSummary.Time, output);
                    else
                    {
                        testResult = new TestFailed(TestCase, DisplayName, runSummary.Time, output, exception);
                        runSummary.Failed++;
                    }

                    if (!CancellationTokenSource.IsCancellationRequested)
                        if (!MessageBus.QueueMessage(testResult))
                            CancellationTokenSource.Cancel();
                }

                OnTestFinishing();
            }

            if (!MessageBus.QueueMessage(new TestFinished(TestCase, DisplayName, runSummary.Time, output)))
                CancellationTokenSource.Cancel();

            OnTestFinished();

            return runSummary;
        }

        /// <summary>
        /// Override this method to invoke the test method.
        /// </summary>
        /// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
        /// <returns>Returns the execution time (in seconds) spent running the test method.</returns>
        protected abstract Task<decimal> InvokeTestAsync(ExceptionAggregator aggregator);
    }
}
