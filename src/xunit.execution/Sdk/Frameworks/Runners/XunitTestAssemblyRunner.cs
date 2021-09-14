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
        MaxConcurrencySyncContext syncContext;

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

        /// <inheritdoc/>
        public override void Dispose()
        {
            var disposable = syncContext as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        /// <inheritdoc/>
        protected override string GetTestFrameworkDisplayName()
            => XunitTestFrameworkDiscoverer.DisplayName;

        /// <inheritdoc/>
        protected override string GetTestFrameworkEnvironment()
        {
            Initialize();

            var testCollectionFactory = ExtensibilityPointFactory.GetXunitTestCollectionFactory(DiagnosticMessageSink, collectionBehaviorAttribute, TestAssembly);
            var threadCountText = maxParallelThreads < 0 ? "unlimited" : maxParallelThreads.ToString();

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
            if (MaxConcurrencySyncContext.IsSupported && maxParallelThreads > 0)
            {
                syncContext = new MaxConcurrencySyncContext(maxParallelThreads);
                SetSynchronizationContext(syncContext);
            }
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

            List<Task<RunSummary>> parallel = null;
            List<Func<Task<RunSummary>>> nonParallel = null;
            var summaries = new List<RunSummary>();

            foreach (var collection in OrderTestCollections())
            {
                Func<Task<RunSummary>> task = () => RunTestCollectionAsync(messageBus, collection.Item1, collection.Item2, cancellationTokenSource);

                // attr is null here from our new unit test, but I'm not sure if that's expected or there's a cheaper approach here
                // Current approach is trying to avoid any changes to the abstractions at all
                var attr = collection.Item1.CollectionDefinition?.GetCustomAttributes(typeof(CollectionDefinitionAttribute)).SingleOrDefault();
                if (attr?.GetNamedArgument<bool>(nameof(CollectionDefinitionAttribute.DisableParallelization)) == true)
                {
                    (nonParallel ?? (nonParallel = new List<Func<Task<RunSummary>>>())).Add(task);
                }
                else
                {
                    (parallel ?? (parallel = new List<Task<RunSummary>>())).Add(taskRunner(task));
                }
            }

            if (parallel?.Count > 0)
            {
                foreach (var task in parallel)
                {
                    try
                    {
                        summaries.Add(await task);
                    }
                    catch (TaskCanceledException) { }
                }
            }

            if (nonParallel?.Count > 0)
            {
                foreach (var task in nonParallel)
                {
                    try
                    {
                        summaries.Add(await taskRunner(task));
                        if (cancellationTokenSource.IsCancellationRequested)
                            break;
                    }
                    catch (TaskCanceledException) { }
                }
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
