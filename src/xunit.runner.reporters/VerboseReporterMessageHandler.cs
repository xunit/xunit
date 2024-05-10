namespace Xunit.Runner.Reporters
{
    public class VerboseReporterMessageHandler : DefaultRunnerReporterWithTypesMessageHandler
    {
        public VerboseReporterMessageHandler(IRunnerLogger logger)
            : base(logger)
        {
            Execution.TestStartingEvent += args => Logger.LogMessage("    {0} [STARTING]", Escape(args.Message.Test.DisplayName));
            Execution.TestFinishedEvent += args => Logger.LogMessage("    {0} [FINISHED] Time: {1}s", Escape(args.Message.Test.DisplayName), args.Message.ExecutionTime);
        }
    }
}
