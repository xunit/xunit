using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class SilentReporter : IRunnerReporter
    {
        public string Description =>
            "turns off all output messages";

        public bool IsEnvironmentallyEnabled =>
            false;

        public string RunnerSwitch =>
            "silent";

        public IMessageSink CreateMessageHandler(IRunnerLogger logger) =>
            new SilentReporterMessageHandler();
    }
}
