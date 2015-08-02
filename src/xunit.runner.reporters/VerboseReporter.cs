using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class VerboseReporter : IRunnerReporter
    {
        public string Description
            => "show verbose progress messages";

        public bool IsEnvironmentallyEnabled
            => false;

        public string RunnerSwitch
            => "verbose";

        public IMessageSink CreateMessageHandler(IRunnerLogger logger)
            => new VerboseReporterMessageHandler(logger);
    }
}
