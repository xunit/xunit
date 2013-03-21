using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestCaseDiscoveryMessage : LongLivedMarshalByRefObject, ITestCaseDiscoveryMessage
    {
        public ITestCase TestCase { get; set; }
    }
}
