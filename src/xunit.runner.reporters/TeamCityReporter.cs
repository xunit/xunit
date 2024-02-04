using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class TeamCityReporter : IRunnerReporter
    {
        public string Description
            => "forces TeamCity mode (normally auto-detected)";

        public bool IsEnvironmentallyEnabled
            => !string.IsNullOrWhiteSpace(EnvironmentHelper.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME"));

        public string RunnerSwitch
            => "teamcity";

        public IMessageSink CreateMessageHandler(IRunnerLogger logger)
            => new TeamCityReporterMessageHandler(logger, EnvironmentHelper.GetEnvironmentVariable("TEAMCITY_PROCESS_FLOW_ID"));
    }
}
