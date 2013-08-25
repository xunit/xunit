using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestStarting"/>.
    /// </summary>
    public class TestStarting : TestMessage, ITestStarting
    {
        public TestStarting(ITestCase testCase, string testDisplayName)
            : base(testCase, testDisplayName) { }
    }
}