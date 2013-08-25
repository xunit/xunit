using System;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestCaseMessage : TestCollectionMessage, ITestCaseMessage
    {
        public TestCaseMessage(ITestCase testCase)
            : base(testCase.TestCollection)
        {
            TestCase = testCase;
        }

        /// <inheritdoc/>
        public ITestCase TestCase { get; private set; }
    }
}
