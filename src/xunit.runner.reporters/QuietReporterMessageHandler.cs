namespace Xunit.Runner.Reporters
{
    public class QuietReporterMessageHandler : DefaultRunnerReporterMessageHandler
    {
        public QuietReporterMessageHandler(IRunnerLogger logger) : base(logger) { }

        protected override bool Visit(ITestAssemblyDiscoveryStarting discoveryStarting)
            => true;

        protected override bool Visit(ITestAssemblyDiscoveryFinished discoveryFinished)
            => true;

        protected override bool Visit(ITestAssemblyExecutionStarting executionStarting)
            => true;

        protected override bool Visit(ITestAssemblyExecutionFinished executionFinished)
            => true;

        protected override bool Visit(ITestExecutionSummary executionSummary)
            => true;
    }
}
