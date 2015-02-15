using Microsoft.Build.Utilities;
using Xunit.Abstractions;

namespace Xunit.Runner.MSBuild
{
    public class DiagnosticMessageVisitor : TestMessageVisitor
    {
        readonly string assemblyDisplayName;
        readonly TaskLoggingHelper log;
        readonly bool showDiagnostics;

        public DiagnosticMessageVisitor(TaskLoggingHelper log, string assemblyDisplayName, bool showDiagnostics)
        {
            this.log = log;
            this.assemblyDisplayName = assemblyDisplayName;
            this.showDiagnostics = showDiagnostics;
        }

        protected override bool Visit(IDiagnosticMessage diagnosticMessage)
        {
            if (showDiagnostics)
                log.LogWarning("{0}: {1}", assemblyDisplayName, diagnosticMessage.Message);

            return base.Visit(diagnosticMessage);
        }
    }
}
