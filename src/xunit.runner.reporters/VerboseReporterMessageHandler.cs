using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class VerboseReporterMessageHandler : DefaultRunnerReporterMessageHandler
    {
        public VerboseReporterMessageHandler(IRunnerLogger logger) : base(logger) { }

        protected override bool Visit(ITestPassed testPassed)
        {
            Logger.LogMessage("   PASS:  {0}", Escape(testPassed.Test.DisplayName));

            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            Logger.LogMessage("   START: {0}", Escape(testStarting.Test.DisplayName));

            return base.Visit(testStarting);
        }

    }
}
