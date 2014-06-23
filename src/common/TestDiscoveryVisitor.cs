using System;
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
        public readonly HashSet<string> KnownTraitNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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