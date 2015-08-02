using System;
using Xunit.Abstractions;

namespace Xunit.ConsoleClient
{
    public class DiagnosticMessageVisitor : TestMessageVisitor
    {
        readonly string assemblyDisplayName;
        readonly object consoleLock;
        readonly bool noColor;
        readonly bool showDiagnostics;

        public DiagnosticMessageVisitor(object consoleLock, string assemblyDisplayName, bool showDiagnostics, bool noColor)
        {
            this.consoleLock = consoleLock;
            this.assemblyDisplayName = assemblyDisplayName;
            this.showDiagnostics = showDiagnostics;
            this.noColor = noColor;
        }

        protected override bool Visit(IDiagnosticMessage diagnosticMessage)
        {
            if (showDiagnostics)
                lock (consoleLock)
                {
                    if (!noColor)
                        Console.ForegroundColor = ConsoleColor.Yellow;

                    Console.WriteLine($"   {assemblyDisplayName}: {diagnosticMessage.Message}");

                    if (!noColor)
                        Console.ResetColor();
                }

            return base.Visit(diagnosticMessage);
        }
    }
}
