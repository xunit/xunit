namespace Xunit.Runner.Reporters
{
    public class QuietReporterMessageHandler : DefaultRunnerReporterMessageHandler
    {
        public QuietReporterMessageHandler(IRunnerLogger logger) : base(logger)
        {
            TestAssemblyDiscoveryStartingEvent -= HandleTestAssemblyDiscoveryStarting;
            TestAssemblyDiscoveryFinishedEvent -= HandleTestAssemblyDiscoveryFinished;
            TestAssemblyExecutionStartingEvent -= HandleTestAssemblyExecutionStarting;
            TestAssemblyExecutionFinishedEvent -= HandleTestAssemblyExecutionFinished;
            TestExecutionSummaryEvent -= HandleTestExecutionSummary;
        }
    }
}
