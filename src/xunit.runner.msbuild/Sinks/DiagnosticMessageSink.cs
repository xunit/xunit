using Microsoft.Build.Utilities;

namespace Xunit.Runner.MSBuild
{
    public class DiagnosticMessageSink : TestMessageSink
    {
        DiagnosticMessageSink() { }

        public static DiagnosticMessageSink ForDiagnostics(TaskLoggingHelper log, string assemblyDisplayName, bool showDiagnostics)
        {
            var result = new DiagnosticMessageSink();

            if (showDiagnostics)
                result.Diagnostics.DiagnosticMessageEvent += args => log.LogWarning("{0}: {1}", assemblyDisplayName, args.Message.Message);

            return result;
        }

        public static DiagnosticMessageSink ForInternalDiagnostics(TaskLoggingHelper log, bool showDiagnostics)
        {
            var result = new DiagnosticMessageSink();

            if (showDiagnostics)
                result.Diagnostics.DiagnosticMessageEvent += args => log.LogMessage("{0}", args.Message.Message);

            return result;
        }
    }
}
