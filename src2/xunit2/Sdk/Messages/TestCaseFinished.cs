using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestCaseFinished : LongLivedMarshalByRefObject, ITestCaseFinished
    {
        public IAssemblyInfo Assembly { get; set; }
        public decimal ExecutionTime { get; set; }
        public ITestCase TestCase { get; set; }
        public int TestsFailed { get; set; }
        public int TestsRun { get; set; }
        public int TestsSkipped { get; set; }
    }
}
