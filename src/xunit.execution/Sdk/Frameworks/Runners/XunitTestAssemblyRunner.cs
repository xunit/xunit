using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The test assembly runner for xUnit.net v2 tests.
    /// </summary>
    public class XunitTestAssemblyRunner : TestAssemblyRunner<IXunitTestCase>
    {
        IAttributeInfo collectionBehaviorAttribute;
        bool disableParallelization;
        bool initialized;
        int maxParallelThreads;
        TaskScheduler scheduler;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestAssemblyRunner"/> class.
        /// </summary>
        /// <param name="testAssembly">The assembly that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="messageSink">The message sink to report run status to.</param>
        /// <param name="executionOptions">The user's requested execution options.</param>
        public XunitTestAssemblyRunner(ITestAssembly testAssembly,
                                       IEnumerable<IXunitTestCase> testCases,
                                       IMessageSink messageSink,
                                       ITestFrameworkOptions executionOptions)
            : base(testAssembly, testCases, messageSink, executionOptions) { }

        /// <inheritdoc/>
        public override void Dispose()
        {
            var disposable = scheduler as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        /// <inheritdoc/>
        protected override string GetTestFrameworkDisplayName()
        {
            return XunitTestFrameworkDiscoverer.DisplayName;
        }

        /// <inheritdoc/>
        protected override string GetTestFrameworkEnvironment()
        {
            Initialize();

            var testCollectionFactory = ExtensibilityPointFactory.GetXunitTestCollectionFactory(collectionBehaviorAttribute, TestAssembly);

            return String.Format("{0}-bit .NET {1} [{2}, {3}{4}]",
                                 IntPtr.Size * 8,
                                 GetVersion(),
                                 testCollectionFactory.DisplayName,
                                 disableParallelization ? "non-parallel" : "parallel",
                                 maxParallelThreads > 0 ? String.Format(" ({0} threads)", maxParallelThreads) : "");
        }

        /// <summary>
        /// Gets the task scheduler used when potentially running tests in parallel.
        /// If <paramref name="maxParallelThreads"/> is greater than 0, it creates
        /// and returns an instance of <see cref="MaxConcurrencyTaskScheduler"/>;
        /// otherwise, it uses the default task scheduler (which runs tasks on
        /// the thread pool).
        /// </summary>
        /// <param name="maxParallelThreads">The maximum number of parallel threads.</param>
        /// <returns>The task scheduler.</returns>
        protected virtual TaskScheduler GetTaskScheduler(int maxParallelThreads)
        {
            if (maxParallelThreads > 0)
                return new MaxConcurrencyTaskScheduler(maxParallelThreads);

            return TaskScheduler.Current;
        }

        private static object GetVersion()
        {
#if WINDOWS_PHONE_APP
            var attr = typeof(object).GetTypeInfo().Assembly.GetCustomAttribute<TargetFrameworkAttribute>();
            return attr == null ? ".NET Framework" : attr.FrameworkDisplayName;
#else
            return Environment.Version;
#endif
        }

        void Initialize()
        {
            if (initialized)
                return;

            collectionBehaviorAttribute = TestAssembly.Assembly.GetCustomAttributes(typeof(CollectionBehaviorAttribute)).SingleOrDefault();
            if (collectionBehaviorAttribute != null)
            {
                disableParallelization = collectionBehaviorAttribute.GetNamedArgument<bool>("DisableTestParallelization");
                maxParallelThreads = collectionBehaviorAttribute.GetNamedArgument<int>("MaxParallelThreads");
            }

            disableParallelization = ExecutionOptions.GetValue<bool>(TestOptionsNames.Execution.DisableParallelization, disableParallelization);
            var maxParallelThreadsOption = ExecutionOptions.GetValue<int>(TestOptionsNames.Execution.MaxParallelThreads, 0);
            if (maxParallelThreadsOption > 0)
                maxParallelThreads = maxParallelThreadsOption;

            scheduler = GetTaskScheduler(maxParallelThreads);

            var ordererAttribute = TestAssembly.Assembly.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
            if (ordererAttribute != null)
                TestCaseOrderer = ExtensibilityPointFactory.GetTestCaseOrderer(ordererAttribute);

            initialized = true;
        }

        /// <inheritdoc/>
        protected override Task AfterTestAssemblyStartingAsync()
        {
            Initialize();
            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        protected override async Task<RunSummary> RunTestCollectionsAsync(IMessageBus messageBus, CancellationTokenSource cancellationTokenSource)
        {
            if (disableParallelization)
                return await base.RunTestCollectionsAsync(messageBus, cancellationTokenSource).ConfigureAwait(false);

            var tasks = TestCases.GroupBy(tc => tc.TestMethod.TestClass.TestCollection, TestCollectionComparer.Instance)
                                 .Select(collectionGroup => Task.Factory.StartNew(() => RunTestCollectionAsync(messageBus, collectionGroup.Key, collectionGroup, cancellationTokenSource),
                                                                                  cancellationTokenSource.Token,
                                                                                  TaskCreationOptions.DenyChildAttach,
                                                                                  scheduler))
                                 .ToArray();

            var summaries = await Task.WhenAll(tasks.Select(t => t.Unwrap())).ConfigureAwait(false);

            return new RunSummary()
            {
                Total = summaries.Sum(s => s.Total),
                Failed = summaries.Sum(s => s.Failed),
                Skipped = summaries.Sum(s => s.Skipped)
            };
        }

        /// <inheritdoc/>
        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, CancellationTokenSource cancellationTokenSource)
        {
            return new XunitTestCollectionRunner(testCollection, testCases, messageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), cancellationTokenSource).RunAsync();
        }
    }
}
