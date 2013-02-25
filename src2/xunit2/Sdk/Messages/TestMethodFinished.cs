using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestMethodFinished : LongLivedMarshalByRefObject, ITestMethodFinished
    {
        public ITestCase TestCase { get; set; }
        public string TestDisplayName { get; set; }
    }
}
