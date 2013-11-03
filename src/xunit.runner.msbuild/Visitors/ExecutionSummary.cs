namespace Xunit.Runner.MSBuild
{
    public class ExecutionSummary
    {
        public int Total { get; set; }
        public int Failed { get; set; }
        public int Skipped { get; set; }
        public decimal Time { get; set; }
    }
}