namespace Xunit.Runner.VisualStudio
{
    public class DiagnosticMessageSink : DiagnosticEventSink
    {
        public DiagnosticMessageSink(LoggerHelper logger, string assemblyDisplayName, bool showDiagnostics)
        {
            if (showDiagnostics)
                DiagnosticMessageEvent += args => logger.LogWarning("{0}: {1}", assemblyDisplayName, args.Message.Message);
        }
    }
}
