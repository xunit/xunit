#if !DNXCORE50

using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class AppVeyorReporter : IRunnerReporter
    {
        public string Description
        {
            get { return "forces AppVeyor CI mode (normally auto-detected)"; }
        }

        public bool IsEnvironmentallyEnabled
        {
            get { return Environment.GetEnvironmentVariable("APPVEYOR_API_URL") != null; }
        }

        public string RunnerSwitch
        {
            get { return "appveyor"; }
        }

        public IMessageSink CreateMessageHandler(IRunnerLogger logger)
        {
            return new AppVeyorReporterMessageHandler(logger);
        }
    }
}

#endif
