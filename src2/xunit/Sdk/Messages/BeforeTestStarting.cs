using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class BeforeTestStarting : LongLivedMarshalByRefObject, IBeforeTestStarting
    {
        public string AttributeName { get; set; }
        public ITestCase TestCase { get; set; }
        public string TestDisplayName { get; set; }
    }
}
