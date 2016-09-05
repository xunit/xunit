using System.Collections.Generic;
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
        static HashSet<string> TestAssemblyFinishedTypes = new HashSet<string>(typeof(TestAssemblyFinished).GetInterfaces().Select(i => i.FullName));
        static HashSet<string> TestCollectionFinishedTypes = new HashSet<string>(typeof(TestCollectionFinished).GetInterfaces().Select(i => i.FullName));
        static HashSet<string> TestFailedTypes = new HashSet<string>(typeof(TestFailed).GetInterfaces().Select(i => i.FullName));

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
        public override bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
        {
            var testSkipped = Cast<ITestSkipped>(message, messageTypes);
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

            var testCollectionFinished = Cast<ITestCollectionFinished>(message, messageTypes);
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

            var assemblyFinished = Cast<ITestAssemblyFinished>(message, messageTypes);
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
