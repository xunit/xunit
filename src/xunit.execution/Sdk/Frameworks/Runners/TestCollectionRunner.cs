using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public abstract class TestCollectionRunner<TTestCase>
        where TTestCase : ITestCase
    {
        public TestCollectionRunner(IMessageBus messageBus,
                                    ITestCollection testCollection,
                                    IEnumerable<TTestCase> testCases,
                                    ITestCaseOrderer testCaseOrderer,
                                    CancellationTokenSource cancellationTokenSource)
        {
            Aggregator = new ExceptionAggregator();
            MessageBus = messageBus;
            TestCollection = testCollection;
            TestCases = testCases;
            TestCaseOrderer = testCaseOrderer;
            CancellationTokenSource = cancellationTokenSource;
        }

        protected ExceptionAggregator Aggregator { get; set; }

        protected CancellationTokenSource CancellationTokenSource { get; set; }

        protected IMessageBus MessageBus { get; set; }
        
        protected ITestCaseOrderer TestCaseOrderer { get; set; }
        
        protected IEnumerable<TTestCase> TestCases { get; set; }
        
        protected ITestCollection TestCollection { get; set; }

        protected virtual void OnTestCollectionStarting() { }

        protected virtual void OnTestCollectionFinished() { }

        public async Task<RunSummary> RunAsync()
        {
            OnTestCollectionStarting();

            var collectionSummary = new RunSummary();

            if (MessageBus.QueueMessage(new TestCollectionStarting(TestCollection)))
            {
                foreach (var testCasesByClass in TestCases.GroupBy(tc => tc.Class))
                {
                    var classSummary = await RunTestClassAsync((IReflectionTypeInfo)testCasesByClass.Key, testCasesByClass);
                    collectionSummary.Aggregate(classSummary);

                    if (CancellationTokenSource.IsCancellationRequested)
                        break;
                }
            }

            if (!MessageBus.QueueMessage(new TestCollectionFinished(TestCollection, collectionSummary.Time, collectionSummary.Total, collectionSummary.Failed, collectionSummary.Skipped)))
                CancellationTokenSource.Cancel();

            OnTestCollectionFinished();

            return collectionSummary;
        }

        protected abstract Task<RunSummary> RunTestClassAsync(IReflectionTypeInfo testClass, IEnumerable<TTestCase> testCases);
    }
}
