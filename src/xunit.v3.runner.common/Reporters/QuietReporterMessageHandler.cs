namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerReporterMessageHandler" /> that supports <see cref="QuietReporter" />.
/// </summary>
public class QuietReporterMessageHandler : DefaultRunnerReporterMessageHandler
{
	/// <summary>
	/// Initializes a new instance of the <see cref="QuietReporterMessageHandler" /> class.
	/// </summary>
	/// <param name="logger">The logger used to report messages</param>
	public QuietReporterMessageHandler(IRunnerLogger logger)
		: base(logger)
	{
		Runner.TestAssemblyDiscoveryStartingEvent -= HandleTestAssemblyDiscoveryStarting;
		Runner.TestAssemblyDiscoveryFinishedEvent -= HandleTestAssemblyDiscoveryFinished;
		Runner.TestAssemblyExecutionStartingEvent -= HandleTestAssemblyExecutionStarting;
		Runner.TestAssemblyExecutionFinishedEvent -= HandleTestAssemblyExecutionFinished;
		Runner.TestExecutionSummariesEvent -= HandleTestExecutionSummaries;
	}
}
