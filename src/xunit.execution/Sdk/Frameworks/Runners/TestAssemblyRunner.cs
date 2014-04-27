using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public abstract class TestAssemblyRunner<TTestCase> : IDisposable
        where TTestCase : ITestCase
    {
        public TestAssemblyRunner(IAssemblyInfo assemblyInfo,
                                  IEnumerable<ITestCase> testCases,
                                  IMessageSink messageSink,
                                  ITestFrameworkOptions executionOptions)
        {
            AssemblyInfo = assemblyInfo;
            AssemblyFileName = AssemblyInfo.AssemblyPath;
            TestCases = testCases;
            MessageSink = messageSink;
            ExecutionOptions = executionOptions;
            TestCaseOrderer = new DefaultTestCaseOrderer();
        }

        /// <summary>
        /// Gets the file name of the assembly under test.
        /// </summary>
        protected string AssemblyFileName { get; private set; }

        protected IAssemblyInfo AssemblyInfo { get; set; }

        protected ITestFrameworkOptions ExecutionOptions { get; set; }

        protected IMessageSink MessageSink { get; set; }

        protected IEnumerable<ITestCase> TestCases { get; set; }

        protected ITestCaseOrderer TestCaseOrderer { get; set; }

        public virtual void Dispose() { }

        protected abstract string GetTestFrameworkDisplayName();

        protected abstract string GetTestFrameworkEnvironment();

        protected virtual void OnAssemblyStarting() { }

        protected virtual void OnAssemblyFinished() { }

        /// <summary>
        /// Creates the message bus to be used for test execution. By default, it inspects
        /// the options for the <see cref="TestOptionsNames.Execution.SynchronnousMessageReporting"/>
        /// flag, and if present, creates a message bus that ensures all messages are delivered
        /// on the same thread.
        /// </summary>
        /// <returns>The message bus.</returns>
        protected virtual IMessageBus CreateMessageBus()
        {
            if (ExecutionOptions.GetValue(TestOptionsNames.Execution.SynchronousMessageReporting, false))
                return new SynchronousMessageBus(MessageSink);

            return new MessageBus(MessageSink);
        }

        public async Task<RunSummary> RunAsync()
        {
            OnAssemblyStarting();

            var cancellationTokenSource = new CancellationTokenSource();
            var totalSummary = new RunSummary();
            var currentDirectory = Directory.GetCurrentDirectory();
            var testFrameworkEnvironment = GetTestFrameworkEnvironment();
            var testFrameworkDisplayName = GetTestFrameworkDisplayName();

            using (var messageBus = CreateMessageBus())
            {
                try
                {
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(AssemblyInfo.AssemblyPath));

                    if (messageBus.QueueMessage(new TestAssemblyStarting(AssemblyFileName, AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, DateTime.Now,
                                                                         testFrameworkEnvironment, testFrameworkDisplayName)))
                    {
                        var masterStopwatch = Stopwatch.StartNew();
                        totalSummary = await RunTestCollectionsAsync(messageBus, cancellationTokenSource);
                        // Want clock time, not aggregated run time
                        totalSummary.Time = (decimal)masterStopwatch.Elapsed.TotalSeconds;
                    }
                }
                finally
                {
                    messageBus.QueueMessage(new TestAssemblyFinished(AssemblyInfo, totalSummary.Time, totalSummary.Total, totalSummary.Failed, totalSummary.Skipped));
                    Directory.SetCurrentDirectory(currentDirectory);
                }
            }

            OnAssemblyFinished();

            return totalSummary;
        }

        protected virtual async Task<RunSummary> RunTestCollectionsAsync(IMessageBus messageBus, CancellationTokenSource cancellationTokenSource)
        {
            var summary = new RunSummary();

            foreach (var collectionGroup in TestCases.Cast<TTestCase>().GroupBy(tc => tc.TestCollection, TestCollectionComparer.Instance))
                summary.Aggregate(await RunTestCollectionAsync(messageBus, collectionGroup.Key, collectionGroup, cancellationTokenSource));

            return summary;
        }

        protected abstract Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<TTestCase> testCases, CancellationTokenSource cancellationTokenSource);
    }
}
