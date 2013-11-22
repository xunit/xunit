using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestSkipped"/>.
    /// </summary>
    internal class TestSkipped : TestResultMessage, ITestSkipped
    {
        public TestSkipped(ITestCase testCase, string testDisplayName, string reason)
            : base(testCase, testDisplayName, 0, null)
        {
            Reason = reason;
        }

        /// <inheritdoc/>
        public string Reason { get; private set; }
    }
}