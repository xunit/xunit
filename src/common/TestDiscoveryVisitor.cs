using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    public class TestDiscoveryVisitor : TestMessageVisitor<IDiscoveryCompleteMessage>
    {
        public TestDiscoveryVisitor()
        {
            TestCases = new List<ITestCase>();
        }

        public List<ITestCase> TestCases { get; private set; }

        public override void Dispose()
        {
            TestCases.ForEach(testCase => testCase.Dispose());
            TestCases = null;
        }

        protected override bool Visit(ITestCaseDiscoveryMessage discovery)
        {
            TestCases.Add(discovery.TestCase);

            return true;
        }
    }
}