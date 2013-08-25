using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassDisposeStarting"/>.
    /// </summary>
    public class TestClassDisposeStarting : TestMessage, ITestClassDisposeStarting
    {
        public TestClassDisposeStarting(ITestCase testCase, string testDisplayName)
            : base(testCase, testDisplayName) { }
    }
}