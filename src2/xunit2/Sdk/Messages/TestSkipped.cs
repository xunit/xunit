using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestSkipped"/>.
    /// </summary>
    public class TestSkipped : TestResultMessage, ITestSkipped
    {
        public TestSkipped(ITestCase testCase, string testDisplayName, string reason)
            : base(testCase, testDisplayName, 0)
        {
            Reason = reason;
        }

        /// <inheritdoc/>
        public string Reason { get; private set; }
    }
}