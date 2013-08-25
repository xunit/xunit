using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCaseDiscoveryMessage"/>.
    /// </summary>
    public class TestCaseDiscoveryMessage : TestCaseMessage, ITestCaseDiscoveryMessage
    {
        public TestCaseDiscoveryMessage(ITestCase testCase)
            : base(testCase) { }
    }
}