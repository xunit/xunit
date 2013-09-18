using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCaseStarting"/>.
    /// </summary>
    internal class TestCaseStarting : TestCaseMessage, ITestCaseStarting
    {
        public TestCaseStarting(ITestCase testCase)
            : base(testCase) { }
    }
}