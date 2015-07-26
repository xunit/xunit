using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    class TestDiscoveryVisitor : TestMessageVisitor<IDiscoveryCompleteMessage>
    {
        public TestDiscoveryVisitor()
        {
            TestCases = new List<ITestCase>();
        }

        public List<ITestCase> TestCases { get; private set; }

        protected override bool Visit(ITestCaseDiscoveryMessage testCaseDiscovered)
        {
            TestCases.Add(testCaseDiscovered.TestCase);

            return true;
        }
    }
}