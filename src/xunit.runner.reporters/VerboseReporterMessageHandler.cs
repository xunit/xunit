using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class VerboseReporterMessageHandler : DefaultRunnerReporterMessageHandler
    {
        public VerboseReporterMessageHandler(IRunnerLogger logger) : base(logger) { }

        protected override bool Visit(ITestStarting testStarting)
        {
            Logger.LogMessage("    {0}", Escape(testStarting.Test.DisplayName));

            return base.Visit(testStarting);
        }
    }
}
