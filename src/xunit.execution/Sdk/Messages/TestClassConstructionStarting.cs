using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassConstructionStarting"/>.
    /// </summary>
    internal class TestClassConstructionStarting : TestMessage, ITestClassConstructionStarting
    {
        public TestClassConstructionStarting(ITestCase testCase, string testDisplayName)
            : base(testCase, testDisplayName) { }
    }
}