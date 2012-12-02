using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestFinished : LongLivedMarshalByRefObject, ITestFinished
    {
        public string DisplayName { get; set; }
        public decimal ExecutionTime { get; set; }
        public ITestCase TestCase { get; set; }
    }
}
