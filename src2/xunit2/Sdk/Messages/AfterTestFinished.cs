using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class AfterTestFinished : LongLivedMarshalByRefObject, IAfterTestFinished
    {
        public string AttributeName { get; set; }
        public ITestCase TestCase { get; set; }
        public string TestDisplayName { get; set; }
    }
}
