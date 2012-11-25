using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestAssemblyFinished : ITestAssemblyFinished
    {
        public IAssemblyInfo Assembly { get; set; }
        public decimal ExecutionTime { get; set; }
        public int TestsFailed { get; set; }
        public int TestsRun { get; set; }
        public int TestsSkipped { get; set; }
    }
}
