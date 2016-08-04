using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class AppVeyorReporter : IRunnerReporter
    {
        public string Description
            => "forces AppVeyor CI mode (normally auto-detected)";

        public bool IsEnvironmentallyEnabled
            => EnvironmentHelper.GetEnvironmentVariable("APPVEYOR_API_URL") != null;

        public string RunnerSwitch
            => "appveyor";

        public IMessageSinkWithTypes CreateMessageHandler(IRunnerLogger logger)
        {
            var baseUri = EnvironmentHelper.GetEnvironmentVariable("APPVEYOR_API_URL");
            return baseUri == null
                ? new DefaultRunnerReporterMessageHandler(logger)
                : new AppVeyorReporterMessageHandler(logger, baseUri);
        }
    }
}
