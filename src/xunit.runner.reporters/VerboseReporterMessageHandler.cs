namespace Xunit.Runner.Reporters
{
    public class VerboseReporterMessageHandler : DefaultRunnerReporterWithTypesMessageHandler
    {
        public VerboseReporterMessageHandler(IRunnerLogger logger)
            : base(logger)
        {
            Execution.TestOutputEvent += args => Logger.LogMessage("    {0} [OUTPUT] {1}", Escape(args.Message.Test.DisplayName), Escape(args.Message.Output.TrimEnd('\r', '\n')));
            Execution.TestStartingEvent += args => Logger.LogMessage("    {0} [STARTING]", Escape(args.Message.Test.DisplayName));
            Execution.TestFinishedEvent += args => Logger.LogMessage("    {0} [FINISHED] Time: {1}s", Escape(args.Message.Test.DisplayName), args.Message.ExecutionTime);
        }
    }
}
