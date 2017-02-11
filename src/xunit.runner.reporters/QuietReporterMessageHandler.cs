namespace Xunit.Runner.Reporters
{
    public class QuietReporterMessageHandler : DefaultRunnerReporterWithTypesMessageHandler
    {
        public QuietReporterMessageHandler(IRunnerLogger logger) : base(logger)
        {
            Runner.TestAssemblyDiscoveryStartingEvent -= HandleTestAssemblyDiscoveryStarting;
            Runner.TestAssemblyDiscoveryFinishedEvent -= HandleTestAssemblyDiscoveryFinished;
            Runner.TestAssemblyExecutionStartingEvent -= HandleTestAssemblyExecutionStarting;
            Runner.TestAssemblyExecutionFinishedEvent -= HandleTestAssemblyExecutionFinished;
            Runner.TestExecutionSummaryEvent -= HandleTestExecutionSummary;
        }
    }
}
