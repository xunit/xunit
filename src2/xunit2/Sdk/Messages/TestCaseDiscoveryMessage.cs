using Xunit.Abstractions;

namespace Xunit.Sdk
{
    internal class TestCaseDiscoveryMessage : LongLivedMarshalByRefObject, ITestCaseDiscoveryMessage
    {
        public ITestCase TestCase { get; internal set; }
    }
}
