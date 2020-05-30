using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IRunnerReporter" /> that supplements the default reporter
    /// behavior by printing out the start and finish of each executing test.
    /// </summary>
    public class VerboseReporter : IRunnerReporter
    {
        /// <inheritdoc />
        public string Description
            => "show verbose progress messages";

        /// <inheritdoc />
        public bool IsEnvironmentallyEnabled
            => false;

        /// <inheritdoc />
        public string RunnerSwitch
            => "verbose";

        /// <inheritdoc />
        public IMessageSink CreateMessageHandler(IRunnerLogger logger)
            => new VerboseReporterMessageHandler(logger);
    }
}
