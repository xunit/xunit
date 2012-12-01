using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestClassFinished : ITestClassFinished
    {
        public IAssemblyInfo Assembly { get; set; }
        public string ClassName { get; set; }
        public decimal ExecutionTime { get; set; }
        public int TestsFailed { get; set; }
        public int TestsRun { get; set; }
        public int TestsSkipped { get; set; }
    }
}
