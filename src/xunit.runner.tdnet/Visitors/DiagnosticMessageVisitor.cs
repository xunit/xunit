using TestDriven.Framework;
using Xunit.Abstractions;

namespace Xunit.Runner.TdNet
{
    public class DiagnosticMessageVisitor : TestMessageVisitor
    {
        readonly string assemblyDisplayName;
        readonly ITestListener listener;
        readonly bool showDiagnostics;

        public DiagnosticMessageVisitor(ITestListener listener, string assemblyDisplayName, bool showDiagnostics)
        {
            this.listener = listener;
            this.assemblyDisplayName = assemblyDisplayName;
            this.showDiagnostics = showDiagnostics;
        }

        protected override bool Visit(IDiagnosticMessage diagnosticMessage)
        {
            if (showDiagnostics)
                listener.WriteLine($"{assemblyDisplayName}: {diagnosticMessage.Message}", Category.Warning);

            return base.Visit(diagnosticMessage);
        }
    }
}
