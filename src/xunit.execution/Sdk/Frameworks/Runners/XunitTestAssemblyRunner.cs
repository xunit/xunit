﻿using System;
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
            : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions) { }

        /// <inheritdoc/>
        public override void Dispose()
        {
            var disposable = syncContext as IDisposable;
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

            var testCollectionFactory = ExtensibilityPointFactory.GetXunitTestCollectionFactory(DiagnosticMessageSink, collectionBehaviorAttribute, TestAssembly);

            return string.Format("{0} [{1}, {2}{3}]",
                                 base.GetTestFrameworkEnvironment(),
                                 testCollectionFactory.DisplayName,
                                 disableParallelization ? "non-parallel" : "parallel",
                                 maxParallelThreads > 0 ? string.Format(" ({0} threads)", maxParallelThreads) : "");
        }

        /// <summary>
        /// Gets the synchronization context used when potentially running tests in parallel.
        /// If <paramref name="maxParallelThreads"/> is greater than 0, it creates
        /// and uses an instance of <see cref="MaxConcurrencySyncContext"/>.
        /// </summary>
        /// <param name="maxParallelThreads">The maximum number of parallel threads.</param>
        protected virtual void SetupSyncContext(int maxParallelThreads)
        {
            if (maxParallelThreads < 1)
                maxParallelThreads = Environment.ProcessorCount;

            syncContext = new MaxConcurrencySyncContext(maxParallelThreads);
            SetSynchronizationContext(syncContext);
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
            var maxParallelThreadsOption = ExecutionOptions.MaxParallelThreads() ?? 0;
            if (maxParallelThreadsOption > 0)
                maxParallelThreads = maxParallelThreadsOption;

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
                        DiagnosticMessageSink.OnMessage(new DiagnosticMessage("Could not find type '{0}' in {1} for assembly-level test case orderer", args[0], args[1]));
                    }
                }
                catch (Exception ex)
                {
                    var innerEx = ex.Unwrap();
                    var args = testCaseOrdererAttribute.GetConstructorArguments().Cast<string>().ToList();
                    DiagnosticMessageSink.OnMessage(new DiagnosticMessage("Assembly-level test case orderer '{0}' threw '{1}' during construction: {2}", args[0], innerEx.GetType().FullName, innerEx.StackTrace));
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
                        DiagnosticMessageSink.OnMessage(new DiagnosticMessage("Could not find type '{0}' in {1} for assembly-level test collection orderer", args[0], args[1]));
                    }
                }
                catch (Exception ex)
                {
                    var innerEx = ex.Unwrap();
                    var args = testCollectionOrdererAttribute.GetConstructorArguments().Cast<string>().ToList();
                    DiagnosticMessageSink.OnMessage(new DiagnosticMessage("Assembly-level test collection orderer '{0}' threw '{1}' during construction: {2}", args[0], innerEx.GetType().FullName, innerEx.StackTrace));
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

            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();

            var tasks = OrderTestCollections().Select(
                collection => Task.Factory.StartNew(() => RunTestCollectionAsync(messageBus, collection.Item1, collection.Item2, cancellationTokenSource),
                                                                                 cancellationTokenSource.Token,
                                                                                 TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler,
                                                                                 scheduler).Unwrap()
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
        {
            return new XunitTestCollectionRunner(testCollection, testCases, DiagnosticMessageSink, messageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), cancellationTokenSource).RunAsync();
        }

        [SecuritySafeCritical]
        static void SetSynchronizationContext(SynchronizationContext context)
        {
            SynchronizationContext.SetSynchronizationContext(context);
        }
    }
}
