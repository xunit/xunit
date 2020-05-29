using System;
using Xunit.Abstractions;

namespace Xunit.ConsoleClient
{
    public class DiagnosticMessageSink : MarshalByRefObject, IMessageSink
    {
        readonly string assemblyDisplayName;
        readonly object consoleLock;
        readonly ConsoleColor displayColor;
        readonly bool noColor;
        readonly bool showDiagnostics;

        DiagnosticMessageSink(object consoleLock, string assemblyDisplayName, bool showDiagnostics, bool noColor, ConsoleColor displayColor)
        {
            this.consoleLock = consoleLock;
            this.assemblyDisplayName = assemblyDisplayName;
            this.noColor = noColor;
            this.displayColor = displayColor;
            this.showDiagnostics = showDiagnostics;
        }

        public static DiagnosticMessageSink ForDiagnostics(object consoleLock, string assemblyDisplayName, bool showDiagnostics, bool noColor)
            => new DiagnosticMessageSink(consoleLock, assemblyDisplayName, showDiagnostics, noColor, ConsoleColor.Yellow);

        public static DiagnosticMessageSink ForInternalDiagnostics(object consoleLock, bool showDiagnostics, bool noColor)
            => new DiagnosticMessageSink(consoleLock, null, showDiagnostics, noColor, ConsoleColor.DarkGray);

        public static DiagnosticMessageSink ForInternalDiagnostics(object consoleLock, string assemblyDisplayName, bool showDiagnostics, bool noColor)
            => new DiagnosticMessageSink(consoleLock, assemblyDisplayName, showDiagnostics, noColor, ConsoleColor.DarkGray);

        public bool OnMessage(IMessageSinkMessage message)
        {
            if (showDiagnostics && message is IDiagnosticMessage diagnosticMessage)
            {
                lock (consoleLock)
                {
                    if (!noColor)
                        ConsoleHelper.SetForegroundColor(displayColor);

                    if (assemblyDisplayName != null)
                        Console.WriteLine($"   {assemblyDisplayName}: {diagnosticMessage.Message}");
                    else
                        Console.WriteLine($"   {diagnosticMessage.Message}");

                    if (!noColor)
                        ConsoleHelper.ResetColor();
                }
            }

            return true;
        }

#if NETFRAMEWORK
        [System.Security.SecurityCritical]
        public override sealed object InitializeLifetimeService()
        {
            return null;
        }
#endif
    }
}
