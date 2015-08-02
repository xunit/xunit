using System;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class TeamCityReporter : IRunnerReporter
    {
        public string Description
            => "forces TeamCity mode (normally auto-detected)";

        public bool IsEnvironmentallyEnabled
            => Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

        public string RunnerSwitch
            => "teamcity";

        public IMessageSink CreateMessageHandler(IRunnerLogger logger)
            => new TeamCityReporterMessageHandler(logger);
    }
}
