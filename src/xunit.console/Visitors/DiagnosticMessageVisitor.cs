using System;
using Xunit.Abstractions;

namespace Xunit.ConsoleClient
{
    public class DiagnosticMessageVisitor : TestMessageVisitor2
    {
        readonly string assemblyDisplayName;
        readonly object consoleLock;
        readonly bool noColor;

        public DiagnosticMessageVisitor(object consoleLock, string assemblyDisplayName, bool showDiagnostics, bool noColor)
        {
            this.consoleLock = consoleLock;
            this.assemblyDisplayName = assemblyDisplayName;
            this.noColor = noColor;
            if (showDiagnostics)
                DiagnosticMessageEvent += HandleDiagnosticMessage;
        }

        private void HandleDiagnosticMessage(MessageHandlerArgs<IDiagnosticMessage> args)
        {
            var diagnosticMessage = args.Message;
            lock (consoleLock)
            {
                if (!noColor)
                    Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine($"   {assemblyDisplayName}: {diagnosticMessage.Message}");

                if (!noColor)
                    Console.ResetColor();
            }
        }
    }
}
