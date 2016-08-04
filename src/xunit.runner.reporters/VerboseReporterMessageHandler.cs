using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class VerboseReporterMessageHandler : DefaultRunnerReporterMessageHandler
    {
        public VerboseReporterMessageHandler(IRunnerLogger logger) : base(logger)
        {
            TestStartingEvent += HandleTestStarting;
        }

        protected virtual void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
        {
            var testStarting = args.Message;
            Logger.LogMessage($"    {Escape(testStarting.Test.DisplayName)}");
        }
    }
}
