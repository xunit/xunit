using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCaseStarting"/>.
    /// </summary>
    public class TestCaseStarting : TestCaseMessage, ITestCaseStarting
    {
        public TestCaseStarting(ITestCase testCase)
            : base(testCase) { }
    }
}