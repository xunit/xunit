using System;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class VstsReporter : IRunnerReporter
    {
        public string Description
            => "forces VSTS CI mode (normally auto-detected)";

        public bool IsEnvironmentallyEnabled
            => !string.IsNullOrWhiteSpace(EnvironmentHelper.GetEnvironmentVariable("TF_BUILD")) &&
               !string.IsNullOrWhiteSpace(EnvironmentHelper.GetEnvironmentVariable("VSTS_ACCESS_TOKEN"));

        public string RunnerSwitch
            => "vsts";

        public IMessageSink CreateMessageHandler(IRunnerLogger logger)
        {
            var accessToken = EnvironmentHelper.GetEnvironmentVariable("VSTS_ACCESS_TOKEN");
            var collectionUri = EnvironmentHelper.GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI");
            var teamProject = EnvironmentHelper.GetEnvironmentVariable("SYSTEM_TEAMPROJECT");

            // Build ID is the ID associated with the build number, which we will use to associate the test run with
            var buildId = Convert.ToInt32(EnvironmentHelper.GetEnvironmentVariable("BUILD_BUILDID"));

            var baseUri = $"{collectionUri}{teamProject}/_apis/test/runs";

            return accessToken == null || collectionUri == null || teamProject == null
                ? new DefaultRunnerReporterWithTypesMessageHandler(logger)
                : new VstsReporterMessageHandler(logger, baseUri, accessToken, buildId);
        }
    }
}
