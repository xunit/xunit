using System;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IExecutionVisitor"/> which converts all skipped
    /// tests into failures, wrapping an existing <see cref="IExecutionVisitor"/>
    /// implementation.
    /// </summary>
    [Obsolete("This class has poor performance; please use FailSkipSink instead.")]
    public class FailSkipVisitor : TestMessageVisitor<ITestAssemblyFinished>, IExecutionVisitor
    {
        readonly IExecutionVisitor Visitor;
        int SkipCount;

        /// <summary>
        /// Initializes a new instance of <see cref="FailSkipVisitor"/>.
        /// </summary>
        /// <param name="visitor">The visitor to pass messages onto.</param>
        public FailSkipVisitor(IExecutionVisitor visitor)
        {
            Guard.ArgumentNotNull(nameof(visitor), visitor);

            Visitor = visitor;
        }

        /// <inheritdoc/>
        public ExecutionSummary ExecutionSummary => Visitor.ExecutionSummary;

        /// <inheritdoc/>
        public override bool OnMessage(IMessageSinkMessage message)
        {
            var testSkipped = message as ITestSkipped;
            if (testSkipped != null)
            {
                SkipCount++;
                var testFailed = new TestFailed(testSkipped.Test, 0M, "",
                                                new[] { "FAIL_SKIP" },
                                                new[] { testSkipped.Reason },
                                                new[] { "" },
                                                new[] { -1 });
                return Visitor.OnMessage(testFailed);
            }

            var testCollectionFinished = message as ITestCollectionFinished;
            if (testCollectionFinished != null)
            {
                testCollectionFinished = new TestCollectionFinished(testCollectionFinished.TestCases,
                                                                    testCollectionFinished.TestCollection,
                                                                    testCollectionFinished.ExecutionTime,
                                                                    testCollectionFinished.TestsRun,
                                                                    testCollectionFinished.TestsFailed + testCollectionFinished.TestsSkipped,
                                                                    0);
                return Visitor.OnMessage(testCollectionFinished);
            }

            var assemblyFinished = message as ITestAssemblyFinished;
            if (assemblyFinished != null)
            {
                assemblyFinished = new TestAssemblyFinished(assemblyFinished.TestCases,
                                                            assemblyFinished.TestAssembly,
                                                            assemblyFinished.ExecutionTime,
                                                            assemblyFinished.TestsRun,
                                                            assemblyFinished.TestsFailed + assemblyFinished.TestsSkipped,
                                                            0);
                var result = Visitor.OnMessage(assemblyFinished);
                base.OnMessage(assemblyFinished);
                return result;
            }

            return Visitor.OnMessage(message);
        }
    }
}
