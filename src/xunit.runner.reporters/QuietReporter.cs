using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class QuietReporter : IRunnerReporter
    {
        public string Description
            => "do not show progress messages";

        public bool IsEnvironmentallyEnabled
            => false;

        public string RunnerSwitch
            => "quiet";

        public IMessageSink CreateMessageHandler(IRunnerLogger logger)
            => new QuietReporterMessageHandler(logger);
    }
}
