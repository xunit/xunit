using Xunit.Abstractions;

namespace Xunit.Runner.Common
{
    /// <summary>
    /// An implementation of <see cref="IRunnerReporter" /> that reports results as individual JSON
    /// objects on the console.
    /// </summary>
    public class JsonReporter : IRunnerReporter
    {
        /// <inheritdoc />
        public string Description => "show progress messages in JSON format";

        /// <inheritdoc />
        public bool IsEnvironmentallyEnabled => false;

        /// <inheritdoc />
        public string? RunnerSwitch => "json";

        /// <inheritdoc />
        public IMessageSink CreateMessageHandler(IRunnerLogger logger) =>
            new JsonReporterMessageHandler(logger);
    }
}
