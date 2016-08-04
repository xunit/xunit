using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class JsonReporter : IRunnerReporter
    {
        public string Description
            => "show progress messages in JSON format";

        public bool IsEnvironmentallyEnabled
            => false;

        public string RunnerSwitch
            => "json";

        public IMessageSinkWithTypes CreateMessageHandler(IRunnerLogger logger)
            => new JsonReporterMessageHandler(logger);
    }
}