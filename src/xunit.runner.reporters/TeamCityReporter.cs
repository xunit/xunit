using System;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class TeamCityReporter : IRunnerReporter
    {
        public string Description
        {
            get { return "forces TeamCity mode (normally auto-detected)"; }
        }

        public bool IsEnvironmentallyEnabled
        {
            get { return Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null; }
        }

        public string RunnerSwitch
        {
            get { return "teamcity"; }
        }

        public IMessageSink CreateMessageHandler(IRunnerLogger logger)
        {
            return new TeamCityReporterMessageHandler(logger);
        }
    }
}
