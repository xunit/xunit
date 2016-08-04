using TestDriven.Framework;
using Xunit.Abstractions;

namespace Xunit.Runner.TdNet
{
    public class DiagnosticMessageVisitor : TestMessageVisitor2
    {
        readonly string assemblyDisplayName;
        readonly ITestListener listener;

        public DiagnosticMessageVisitor(ITestListener listener, string assemblyDisplayName, bool showDiagnostics)
        {
            this.listener = listener;
            this.assemblyDisplayName = assemblyDisplayName;

            if (showDiagnostics)
                DiagnosticMessageEvent += HandleDiagnosticMessage;
        }

        private void HandleDiagnosticMessage(MessageHandlerArgs<IDiagnosticMessage> args)
        {
            var diagnosticMessage = args.Message;
            listener.WriteLine($"{assemblyDisplayName}: {diagnosticMessage.Message}", Category.Warning);
        }
    }
}
