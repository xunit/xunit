using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestPassed"/>.
    /// </summary>
    public class TestPassed : TestResultMessage, ITestPassed
    {
        public TestPassed(ITestCase testCase, string testDisplayName, decimal executionTime)
            : base(testCase, testDisplayName, executionTime) { }
    }
}