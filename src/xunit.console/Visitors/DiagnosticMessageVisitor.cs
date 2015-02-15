using System;
using Xunit.Abstractions;

namespace Xunit.ConsoleClient
{
    public class DiagnosticMessageVisitor : TestMessageVisitor
    {
        readonly string assemblyDisplayName;
        readonly object consoleLock;
        readonly bool showDiagnostics;

        public DiagnosticMessageVisitor(object consoleLock, string assemblyDisplayName, bool showDiagnostics)
        {
            this.consoleLock = consoleLock;
            this.assemblyDisplayName = assemblyDisplayName;
            this.showDiagnostics = showDiagnostics;
        }

        protected override bool Visit(IDiagnosticMessage diagnosticMessage)
        {
            if (showDiagnostics)
                lock (consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("   {0}: {1}", assemblyDisplayName, diagnosticMessage.Message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

            return base.Visit(diagnosticMessage);
        }
    }
}
