using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary/>
    [Obsolete("This class has poor performance; please use TestDiscoverySink instead.")]
    public class TestDiscoveryVisitor : TestMessageVisitor<IDiscoveryCompleteMessage>
    {
        /// <summary/>
        public TestDiscoveryVisitor()
        {
            TestCases = new List<ITestCase>();
        }

        /// <summary/>
        public List<ITestCase> TestCases { get; private set; }

        /// <inheritdoc/>
        protected override bool Visit(ITestCaseDiscoveryMessage testCaseDiscovered)
        {
            TestCases.Add(testCaseDiscovered.TestCase);

            return true;
        }
    }
}
