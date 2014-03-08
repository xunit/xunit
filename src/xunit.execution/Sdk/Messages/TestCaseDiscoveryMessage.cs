using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCaseDiscoveryMessage"/>.
    /// </summary>
    internal class TestCaseDiscoveryMessage : TestCaseMessage, ITestCaseDiscoveryMessage
    {
        public TestCaseDiscoveryMessage(ITestCase testCase)
            : base(testCase) { }
    }
}