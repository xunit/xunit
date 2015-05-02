using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Xunit.ConsoleClient
{
    public class XmlConsoleTestExecutionVisitor : XmlTestExecutionVisitor
    {
        protected readonly bool noskips;

        public XmlConsoleTestExecutionVisitor(bool noskips, XElement assemblyElement, Func<bool> cancelThunk)
            : base(assemblyElement, cancelThunk)
        {
            this.noskips = noskips;
        }

        protected override bool Visit(Abstractions.ITestAssemblyFinished assemblyFinished)
        {
            if (noskips)
                Failed += assemblyFinished.TestsSkipped;

            return base.Visit(assemblyFinished);
        }

        protected override bool Visit(Abstractions.ITestCollectionFinished testCollectionFinished)
        {
            if (noskips)
                testCollectionFinished = new TestCollectionFinished(testCollectionFinished.TestCases,
                                                                    testCollectionFinished.TestCollection,
                                                                    testCollectionFinished.ExecutionTime,
                                                                    testCollectionFinished.TestsRun,
                                                                    testCollectionFinished.TestsFailed + testCollectionFinished.TestsSkipped,
                                                                    testCollectionFinished.TestsSkipped);

            return base.Visit(testCollectionFinished);
        }
    }
}
