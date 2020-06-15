using System;
using Xunit.Abstractions;

namespace Xunit.Runner.Common
{
    /// <summary>
    /// An implementation of <see cref="IRunnerReporter" /> that reports results to Azure DevOps/VSTS.
    /// This is auto-enabled by the presence of four required environment variables: "VSTS_ACCESS_TOKEN",
    /// "SYSTEM_TEAMFOUNDATIONCOLLECTIONURI", "SYSTEM_TEAMPROJECT", and "BUILD_BUILDID".
    /// </summary>
    public class VstsReporter : IRunnerReporter
    {
        /// <inheritdoc />
        public string Description
            => "Azure DevOps/VSTS CI support";

        /// <inheritdoc />
        public bool IsEnvironmentallyEnabled
            => !string.IsNullOrWhiteSpace(EnvironmentHelper.GetEnvironmentVariable("VSTS_ACCESS_TOKEN")) &&
               !string.IsNullOrWhiteSpace(EnvironmentHelper.GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI")) &&
               !string.IsNullOrWhiteSpace(EnvironmentHelper.GetEnvironmentVariable("SYSTEM_TEAMPROJECT")) &&
               !string.IsNullOrWhiteSpace(EnvironmentHelper.GetEnvironmentVariable("BUILD_BUILDID"));

        /// <inheritdoc />
        public string RunnerSwitch
            => null;

        /// <inheritdoc />
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
