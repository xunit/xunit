using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class QuietReporter : IRunnerReporter
    {
        public string Description
        {
            get { return "do not show progress messages"; }
        }

        public bool IsEnvironmentallyEnabled
        {
            get { return false; }
        }

        public string RunnerSwitch
        {
            get { return "quiet"; }
        }

        public IMessageSink CreateMessageHandler(IRunnerLogger logger)
        {
            return new QuietReporterMessageHandler(logger);
        }
    }
}
