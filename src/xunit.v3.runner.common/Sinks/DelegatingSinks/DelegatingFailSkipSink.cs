using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// A delegating implementation of <see cref="IExecutionSink"/> which converts all
    /// skipped tests into failures before passing them on to the inner sink.
    /// </summary>
    public class DelegatingFailSkipSink : LongLivedMarshalByRefObject, IExecutionSink
    {
        readonly IExecutionSink innerSink;
        int skipCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatingFailSkipSink"/> class.
        /// </summary>
        /// <param name="innerSink">The sink to delegate messages to.</param>
        public DelegatingFailSkipSink(IExecutionSink innerSink)
        {
            Guard.ArgumentNotNull(nameof(innerSink), innerSink);

            this.innerSink = innerSink;
        }

        /// <inheritdoc/>
        public ExecutionSummary ExecutionSummary => innerSink.ExecutionSummary;

        /// <inheritdoc/>
        public ManualResetEvent Finished => innerSink.Finished;

        /// <inheritdoc/>
        public void Dispose()
            => innerSink.Dispose();

        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
        {
            var testSkipped = message.Cast<ITestSkipped>(messageTypes);
            if (testSkipped != null)
            {
                skipCount++;
                var testFailed = new TestFailed(testSkipped.Test, 0M, "",
                                                new[] { "FAIL_SKIP" },
                                                new[] { testSkipped.Reason },
                                                new[] { "" },
                                                new[] { -1 });
                return innerSink.OnMessage(testFailed);
            }

            var testCollectionFinished = message.Cast<ITestCollectionFinished>(messageTypes);
            if (testCollectionFinished != null)
            {
                testCollectionFinished = new TestCollectionFinished(testCollectionFinished.TestCases,
                                                                    testCollectionFinished.TestCollection,
                                                                    testCollectionFinished.ExecutionTime,
                                                                    testCollectionFinished.TestsRun,
                                                                    testCollectionFinished.TestsFailed + testCollectionFinished.TestsSkipped,
                                                                    0);
                return innerSink.OnMessage(testCollectionFinished);
            }

            var assemblyFinished = message.Cast<ITestAssemblyFinished>(messageTypes);
            if (assemblyFinished != null)
            {
                assemblyFinished = new TestAssemblyFinished(assemblyFinished.TestCases,
                                                            assemblyFinished.TestAssembly,
                                                            assemblyFinished.ExecutionTime,
                                                            assemblyFinished.TestsRun,
                                                            assemblyFinished.TestsFailed + assemblyFinished.TestsSkipped,
                                                            0);
                return innerSink.OnMessage(assemblyFinished);
            }

            return innerSink.OnMessageWithTypes(message, messageTypes);
        }
    }
}
