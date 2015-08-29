using Xunit.Abstractions;

namespace Xunit
{
    public class FailSkipVisitor : TestMessageVisitor<ITestAssemblyFinished>, IExecutionVisitor
    {
        readonly IExecutionVisitor Visitor;
        int SkipCount;

        public FailSkipVisitor(IExecutionVisitor visitor)
        {
            Visitor = visitor;
        }

        public ExecutionSummary ExecutionSummary => Visitor.ExecutionSummary;

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
