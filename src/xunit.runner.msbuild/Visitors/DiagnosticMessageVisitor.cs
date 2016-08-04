using Microsoft.Build.Utilities;
using Xunit.Abstractions;

namespace Xunit.Runner.MSBuild
{
    public class DiagnosticMessageVisitor : TestMessageVisitor2
    {
        readonly string assemblyDisplayName;
        readonly TaskLoggingHelper log;

        public DiagnosticMessageVisitor(TaskLoggingHelper log, string assemblyDisplayName, bool showDiagnostics)
        {
            this.log = log;
            this.assemblyDisplayName = assemblyDisplayName;
            if (showDiagnostics)
                this.DiagnosticMessageEvent += HandleDiagnosticMessage;
        }

        private void HandleDiagnosticMessage(MessageHandlerArgs<IDiagnosticMessage> args)
        {
            var diagnosticMessage = args.Message;
            log.LogWarning("{0}: {1}", assemblyDisplayName, diagnosticMessage.Message);
        }
    }
}
