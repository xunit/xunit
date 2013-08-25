using System;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestMessage : TestCaseMessage, ITestMessage
    {
        public TestMessage(ITestCase testCase, string testDisplayName)
            : base(testCase)
        {
            TestDisplayName = testDisplayName;
        }

        /// <inheritdoc/>
        public string TestDisplayName { get; private set; }
    }
}
