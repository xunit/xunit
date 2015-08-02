using System;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class AppVeyorReporter : IRunnerReporter
    {
        public string Description
            => "forces AppVeyor CI mode (normally auto-detected)";

        public bool IsEnvironmentallyEnabled
            => Environment.GetEnvironmentVariable("APPVEYOR_API_URL") != null;

        public string RunnerSwitch
            => "appveyor";

        public IMessageSink CreateMessageHandler(IRunnerLogger logger)
            => new AppVeyorReporterMessageHandler(logger);
    }
}
