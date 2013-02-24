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

        protected override void Visit(ITestCaseDiscoveryMessage discovery)
        {
            TestCases.Add(discovery.TestCase);
        }
    }
}