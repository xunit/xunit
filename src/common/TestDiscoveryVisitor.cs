using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    internal class TestDiscoveryVisitor : TestMessageVisitor<IDiscoveryCompleteMessage>
    {
        public TestDiscoveryVisitor()
        {
            TestCases = new List<ITestCase>();
        }

        public List<ITestCase> TestCases { get; private set; }

        public override void Dispose()
        {
            foreach (var testCase in TestCases) testCase.Dispose();
            TestCases = null;
        }

        protected override bool Visit(ITestCaseDiscoveryMessage discovery)
        {
            TestCases.Add(discovery.TestCase);

            return true;
        }
    }
}