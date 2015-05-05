using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.ConsoleClient
{
    public class NoSkipVisitor : TestMessageVisitor<ITestAssemblyFinished>
    {
        readonly XmlTestExecutionVisitor Visitor;
        int SkipCount;

        public NoSkipVisitor(XmlTestExecutionVisitor visitor)
        {
            Visitor = visitor;
        }

        public override bool OnMessage(IMessageSinkMessage message)
        {
            var testSkipped = message as ITestSkipped;
            if (testSkipped != null)
            {
                SkipCount++;
                var testFailed = new TestFailed(testSkipped.Test, testSkipped.ExecutionTime, testSkipped.Reason,
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
                                                                    testsSkipped: 0);
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
                                                            testsSkipped: 0);
                var result = Visitor.OnMessage(assemblyFinished);
                base.OnMessage(assemblyFinished);
                return result;
            }

            return Visitor.OnMessage(message);
        }
    }
}
