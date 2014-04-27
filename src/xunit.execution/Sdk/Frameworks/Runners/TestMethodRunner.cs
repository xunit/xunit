using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public abstract class TestMethodRunner<TTestCase>
            where TTestCase : ITestCase
    {
        public TestMethodRunner(IMessageBus messageBus,
                                ITestCollection testCollection,
                                IReflectionTypeInfo testClass,
                                IReflectionMethodInfo testMethod,
                                IEnumerable<TTestCase> testCases,
                                CancellationTokenSource cancellationTokenSource)
        {
            MessageBus = messageBus;
            TestCollection = testCollection;
            TestClass = testClass;
            TestMethod = testMethod;
            TestCases = testCases;
            CancellationTokenSource = cancellationTokenSource;
        }

        protected CancellationTokenSource CancellationTokenSource { get; set; }

        protected IMessageBus MessageBus { get; set; }

        protected IEnumerable<TTestCase> TestCases { get; set; }

        protected IReflectionTypeInfo TestClass { get; set; }

        protected ITestCollection TestCollection { get; set; }

        protected IReflectionMethodInfo TestMethod { get; set; }

        protected virtual void OnTestMethodFinished() { }

        protected virtual void OnTestMethodStarting() { }

        protected abstract Task<RunSummary> RunTestCaseAsync(TTestCase testCase);

        public async Task<RunSummary> RunTestMethodAsync()
        {
            OnTestMethodStarting();

            var methodSummary = new RunSummary();

            if (!MessageBus.QueueMessage(new TestMethodStarting(TestCollection, TestClass.Name, TestMethod.Name)))
                CancellationTokenSource.Cancel();
            else
            {
                foreach (var testCase in TestCases)
                {
                    var testCaseSummary = await RunTestCaseAsync(testCase);
                    methodSummary.Aggregate(testCaseSummary);
                    if (CancellationTokenSource.IsCancellationRequested)
                        break;
                }
            }

            if (!MessageBus.QueueMessage(new TestMethodFinished(TestCollection, TestClass.Name, TestMethod.Name)))
                CancellationTokenSource.Cancel();

            OnTestMethodFinished();

            return methodSummary;
        }
    }
}
