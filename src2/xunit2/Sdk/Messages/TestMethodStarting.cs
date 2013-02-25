using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestMethodStarting : LongLivedMarshalByRefObject, ITestMethodStarting
    {
        public ITestCase TestCase { get; set; }
        public string TestDisplayName { get; set; }
    }
}
