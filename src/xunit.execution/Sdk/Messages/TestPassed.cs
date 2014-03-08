using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestPassed"/>.
    /// </summary>
    internal class TestPassed : TestResultMessage, ITestPassed
    {
        public TestPassed(ITestCase testCase, string testDisplayName, decimal executionTime, string output)
            : base(testCase, testDisplayName, executionTime, output) { }
    }
}