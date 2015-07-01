using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class VerboseReporter : IRunnerReporter
    {
        public string Description
        {
            get { return "show verbose progress messages"; }
        }

        public bool IsEnvironmentallyEnabled
        {
            get { return false; }
        }

        public string RunnerSwitch
        {
            get { return "verbose"; }
        }

        public IMessageSink CreateMessageHandler(IRunnerLogger logger)
        {
            return new VerboseReporterMessageHandler(logger);
        }
    }
}
