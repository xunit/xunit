using Microsoft.Build.Utilities;

namespace Xunit.Runner.MSBuild
{
    public class DiagnosticMessageSink : TestMessageSink
    {
        public DiagnosticMessageSink(TaskLoggingHelper log, string assemblyDisplayName, bool showDiagnostics)
        {
            if (showDiagnostics)
                Diagnostics.DiagnosticMessageEvent += args => log.LogWarning("{0}: {1}", assemblyDisplayName, args.Message.Message);
        }
    }
}
