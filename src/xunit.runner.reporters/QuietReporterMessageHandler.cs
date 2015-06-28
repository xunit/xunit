using Xunit.Abstractions;

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

        protected override bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            return true;
        }

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            return true;
        }

        protected override bool Visit(ITestExecutionSummary executionSummary)
        {
            return true;
        }
    }
}
