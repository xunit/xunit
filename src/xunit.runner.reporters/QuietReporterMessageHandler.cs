namespace Xunit.Runner.Reporters
{
    public class QuietReporterMessageHandler : DefaultRunnerReporterMessageHandler
    {
        public QuietReporterMessageHandler(IRunnerLogger logger) : base(logger) { }

        protected override bool Visit(ITestAssemblyDiscoveryStarting discoveryStarting)
        {
            return true;
        }

        protected override bool Visit(ITestAssemblyDiscoveryFinished discoveryFinished)
        {
            return true;
        }

        protected override bool Visit(ITestAssemblyExecutionStarting executionStarting)
        {
            return true;
        }

        protected override bool Visit(ITestAssemblyExecutionFinished executionFinished)
        {
            return true;
        }

        protected override bool Visit(ITestExecutionSummary executionSummary)
        {
            return true;
        }
    }
}
