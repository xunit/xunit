using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IMessageSink" /> and <see cref="IMessageSinkWithTypes" /> that
    /// supports <see cref="QuietReporter" />.
    /// </summary>
    public class QuietReporterMessageHandler : DefaultRunnerReporterWithTypesMessageHandler
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
            Runner.TestExecutionSummaryEvent -= HandleTestExecutionSummary;
        }
    }
}
