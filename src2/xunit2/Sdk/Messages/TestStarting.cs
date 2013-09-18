using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestStarting"/>.
    /// </summary>
    internal class TestStarting : TestMessage, ITestStarting
    {
        public TestStarting(ITestCase testCase, string testDisplayName)
            : base(testCase, testDisplayName) { }
    }
}