using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
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
        SynchronizationContext originalSyncContext;

#if !DOTNETCORE
        MaxConcurrencySyncContext syncContext;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestAssemblyRunner"/> class.
        /// </summary>
        /// <param name="testAssembly">The assembly that contains the tests to be run.</param>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="diagnosticMessageSink">The message sink to report diagnostic messages to.</param>
        /// <param name="executionMessageSink">The message sink to report run status to.</param>
        /// <param name="executionOptions">The user's requested execution options.</param>
        public XunitTestAssemblyRunner(ITestAssembly testAssembly,
                                       IEnumerable<IXunitTestCase> testCases,
                                       IMessageSink diagnosticMessageSink,
                                       IMessageSink executionMessageSink,
                                       ITestFrameworkExecutionOptions executionOptions)
            : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
        { }

#if !DOTNETCORE
        /// <inheritdoc/>
        public override void Dispose()
        {
            var disposable = syncContext as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
#endif

        /// <inheritdoc/>
        protected override string GetTestFrameworkDisplayName()
            => XunitTestFrameworkDiscoverer.DisplayName;

        /// <inheritdoc/>
        protected override string GetTestFrameworkEnvironment()
        {
            Initialize();

            var testCollectionFactory = ExtensibilityPointFactory.GetXunitTestCollectionFactory(DiagnosticMessageSink, collectionBehaviorAttribute, TestAssembly);

#if DOTNETCORE
            var threadCountText = "unlimited";
#else
            var threadCountText = maxParallelThreads < 0 ? "unlimited" : maxParallelThreads.ToString();
#endif

            return $"{base.GetTestFrameworkEnvironment()} [{testCollectionFactory.DisplayName}, {(disableParallelization ? "non-parallel" : $"parallel ({threadCountText} threads)")}]";
        }

        /// <summary>
        /// Gets the synchronization context used when potentially running tests in parallel.
        /// If <paramref name="maxParallelThreads"/> is greater than 0, it creates
        /// and uses an instance of <see cref="T:Xunit.Sdk.MaxConcurrencySyncContext"/>.
        /// </summary>
        /// <param name="maxParallelThreads">The maximum number of parallel threads.</param>
        protected virtual void SetupSyncContext(int maxParallelThreads)
        {
#if !DOTNETCORE
            if (maxParallelThreads > 0)
            {
                syncContext = new MaxConcurrencySyncContext(maxParallelThreads);
                SetSynchronizationContext(syncContext);
            }
#endif
        }

        /// <summary>
        /// Ensures the assembly runner is initialized (sets up the collection behavior,
        /// parallelization options, and test orderers from their assembly attributes).
        /// </summary>
        protected void Initialize()
        {
            if (initialized)
                return;

            collectionBehaviorAttribute = TestAssembly.Assembly.GetCustomAttributes(typeof(CollectionBehaviorAttribute)).SingleOrDefault();
            if (collectionBehaviorAttribute != null)
            {
                disableParallelization = collectionBehaviorAttribute.GetNamedArgument<bool>("DisableTestParallelization");
                maxParallelThreads = collectionBehaviorAttribute.GetNamedArgument<int>("MaxParallelThreads");
            }

            disableParallelization = ExecutionOptions.DisableParallelization() ?? disableParallelization;
            maxParallelThreads = ExecutionOptions.MaxParallelThreads() ?? maxParallelThreads;
            if (maxParallelThreads == 0)
                maxParallelThreads = Environment.ProcessorCount;

            var testCaseOrdererAttribute = TestAssembly.Assembly.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
            if (testCaseOrdererAttribute != null)
            {
                try
                {
                    var testCaseOrderer = ExtensibilityPointFactory.GetTestCaseOrderer(DiagnosticMessageSink, testCaseOrdererAttribute);
                    if (testCaseOrderer != null)
                        TestCaseOrderer = testCaseOrderer;
                    else
                    {
                        var args = testCaseOrdererAttribute.GetConstructorArguments().Cast<string>().ToList();
                        DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"Could not find type '{args[0]}' in {args[1]} for assembly-level test case orderer"));
                    }
                }
                catch (Exception ex)
                {
                    var innerEx = ex.Unwrap();
                    var args = testCaseOrdererAttribute.GetConstructorArguments().Cast<string>().ToList();
                    DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"Assembly-level test case orderer '{args[0]}' threw '{innerEx.GetType().FullName}' during construction: {innerEx.Message}{Environment.NewLine}{innerEx.StackTrace}"));
                }
            }

            var testCollectionOrdererAttribute = TestAssembly.Assembly.GetCustomAttributes(typeof(TestCollectionOrdererAttribute)).SingleOrDefault();
            if (testCollectionOrdererAttribute != null)
            {
                try
                {
                    var testCollectionOrderer = ExtensibilityPointFactory.GetTestCollectionOrderer(DiagnosticMessageSink, testCollectionOrdererAttribute);
                    if (testCollectionOrderer != null)
                        TestCollectionOrderer = testCollectionOrderer;
                    else
                    {
                        var args = testCollectionOrdererAttribute.GetConstructorArguments().Cast<string>().ToList();
                        DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"Could not find type '{args[0]}' in {args[1]} for assembly-level test collection orderer"));
                    }
                }
                catch (Exception ex)
                {
                    var innerEx = ex.Unwrap();
                    var args = testCollectionOrdererAttribute.GetConstructorArguments().Cast<string>().ToList();
                    DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"Assembly-level test collection orderer '{args[0]}' threw '{innerEx.GetType().FullName}' during construction: {innerEx.Message}{Environment.NewLine}{innerEx.StackTrace}"));
                }
            }

            initialized = true;
        }

        /// <inheritdoc/>
        protected override Task AfterTestAssemblyStartingAsync()
        {
            Initialize();
            return CommonTasks.Completed;
        }

        /// <inheritdoc/>
        protected override Task BeforeTestAssemblyFinishedAsync()
        {
            SetSynchronizationContext(originalSyncContext);
            return CommonTasks.Completed;
        }

        /// <inheritdoc/>
        protected override async Task<RunSummary> RunTestCollectionsAsync(IMessageBus messageBus, CancellationTokenSource cancellationTokenSource)
        {
            originalSyncContext = SynchronizationContext.Current;

            if (disableParallelization)
                return await base.RunTestCollectionsAsync(messageBus, cancellationTokenSource);

            SetupSyncContext(maxParallelThreads);

            Func<Func<Task<RunSummary>>, Task<RunSummary>> taskRunner;
            if (SynchronizationContext.Current != null)
            {
                var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                taskRunner = code => Task.Factory.StartNew(code, cancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap();
            }
            else
                taskRunner = code => Task.Run(code, cancellationTokenSource.Token);

            var tasks = OrderTestCollections().Select(
                collection => taskRunner(() => RunTestCollectionAsync(messageBus, collection.Item1, collection.Item2, cancellationTokenSource))
            ).ToArray();

            var summaries = new List<RunSummary>();

            foreach (var task in tasks)
            {
                try
                {
                    summaries.Add(await task);
                }
                catch (TaskCanceledException) { }
            }

            return new RunSummary()
            {
                Total = summaries.Sum(s => s.Total),
                Failed = summaries.Sum(s => s.Failed),
                Skipped = summaries.Sum(s => s.Skipped)
            };
        }

        /// <inheritdoc/>
        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, CancellationTokenSource cancellationTokenSource)
            => new XunitTestCollectionRunner(testCollection, testCases, DiagnosticMessageSink, messageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), cancellationTokenSource).RunAsync();

        [SecuritySafeCritical]
        static void SetSynchronizationContext(SynchronizationContext context)
            => SynchronizationContext.SetSynchronizationContext(context);
    }
}
