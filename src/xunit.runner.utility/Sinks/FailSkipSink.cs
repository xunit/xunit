using System.Linq;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IExecutionSink"/> which converts all skipped
    /// tests into failures, wrapping an existing <see cref="IExecutionSink"/>
    /// implementation.
    /// </summary>
    public class FailSkipSink : TestMessageSink, IExecutionSink
    {
        static string[] TestAssemblyFinishedTypes = typeof(TestAssemblyFinished).GetInterfaces().Select(i => i.FullName).ToArray();
        static string[] TestCollectionFinishedTypes = typeof(TestCollectionFinished).GetInterfaces().Select(i => i.FullName).ToArray();
        static string[] TestFailedTypes = typeof(TestFailed).GetInterfaces().Select(i => i.FullName).ToArray();

        readonly IExecutionSink Sink;
        int SkipCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="FailSkipSink"/> class.
        /// </summary>
        /// <param name="visitor">The visitor to pass messages onto.</param>
        public FailSkipSink(IExecutionSink visitor)
        {
            Guard.ArgumentNotNull(nameof(visitor), visitor);

            Sink = visitor;
        }

        /// <inheritdoc/>
        public ExecutionSummary ExecutionSummary => Sink.ExecutionSummary;

        /// <inheritdoc/>
        public ManualResetEvent Finished => Sink.Finished;

        /// <inheritdoc/>
        public override bool OnMessageWithTypes(IMessageSinkMessage message, string[] messageTypes)
        {
            var hashedTypes = GetMessageTypesAsHashSet(messageTypes);

            var testSkipped = Cast<ITestSkipped>(message, hashedTypes);
            if (testSkipped != null)
            {
                SkipCount++;
                var testFailed = new TestFailed(testSkipped.Test, 0M, "",
                                                new[] { "FAIL_SKIP" },
                                                new[] { testSkipped.Reason },
                                                new[] { "" },
                                                new[] { -1 });
                return Sink.OnMessageWithTypes(testFailed, TestFailedTypes);
            }

            var testCollectionFinished = Cast<ITestCollectionFinished>(message, hashedTypes);
            if (testCollectionFinished != null)
            {
                testCollectionFinished = new TestCollectionFinished(testCollectionFinished.TestCases,
                                                                    testCollectionFinished.TestCollection,
                                                                    testCollectionFinished.ExecutionTime,
                                                                    testCollectionFinished.TestsRun,
                                                                    testCollectionFinished.TestsFailed + testCollectionFinished.TestsSkipped,
                                                                    0);
                return Sink.OnMessageWithTypes(testCollectionFinished, TestCollectionFinishedTypes);
            }

            var assemblyFinished = Cast<ITestAssemblyFinished>(message, hashedTypes);
            if (assemblyFinished != null)
            {
                assemblyFinished = new TestAssemblyFinished(assemblyFinished.TestCases,
                                                            assemblyFinished.TestAssembly,
                                                            assemblyFinished.ExecutionTime,
                                                            assemblyFinished.TestsRun,
                                                            assemblyFinished.TestsFailed + assemblyFinished.TestsSkipped,
                                                            0);
                return Sink.OnMessageWithTypes(assemblyFinished, TestAssemblyFinishedTypes);
            }

            return Sink.OnMessageWithTypes(message, messageTypes);
        }
    }
}
