using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class BeforeTestFinished : LongLivedMarshalByRefObject, IBeforeTestFinished
    {
        public string AttributeName { get; set; }
        public ITestCase TestCase { get; set; }
        public string TestDisplayName { get; set; }
    }
}
