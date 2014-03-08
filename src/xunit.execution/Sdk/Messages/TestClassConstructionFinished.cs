using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassConstructionFinished"/>.
    /// </summary>
    internal class TestClassConstructionFinished : TestMessage, ITestClassConstructionFinished
    {
        public TestClassConstructionFinished(ITestCase testCase, string testDisplayName)
            : base(testCase, testDisplayName) { }
    }
}