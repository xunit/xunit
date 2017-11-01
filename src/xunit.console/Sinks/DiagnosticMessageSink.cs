using System;
using Xunit.Abstractions;

namespace Xunit.ConsoleClient
{
    public class DiagnosticMessageSink : TestMessageSink
    {
        readonly string assemblyDisplayName;
        readonly object consoleLock;
        readonly ConsoleColor displayColor;
        readonly bool noColor;

        DiagnosticMessageSink(object consoleLock, string assemblyDisplayName, bool showDiagnostics, bool noColor, ConsoleColor displayColor)
        {
            this.consoleLock = consoleLock;
            this.assemblyDisplayName = assemblyDisplayName;
            this.noColor = noColor;
            this.displayColor = displayColor;

            if (showDiagnostics)
                Diagnostics.DiagnosticMessageEvent += HandleDiagnosticMessage;
        }

        public static DiagnosticMessageSink ForDiagnostics(object consoleLock, string assemblyDisplayName, bool showDiagnostics, bool noColor)
            => new DiagnosticMessageSink(consoleLock, assemblyDisplayName, showDiagnostics, noColor, ConsoleColor.Yellow);

        public static DiagnosticMessageSink ForInternalDiagnostics(object consoleLock, bool showDiagnostics, bool noColor)
            => new DiagnosticMessageSink(consoleLock, null, showDiagnostics, noColor, ConsoleColor.DarkGray);

        public static DiagnosticMessageSink ForInternalDiagnostics(object consoleLock, string assemblyDisplayName, bool showDiagnostics, bool noColor)
            => new DiagnosticMessageSink(consoleLock, assemblyDisplayName, showDiagnostics, noColor, ConsoleColor.DarkGray);

        void HandleDiagnosticMessage(MessageHandlerArgs<IDiagnosticMessage> args)
        {
            lock (consoleLock)
            {
                if (!noColor)
                    ConsoleHelper.SetForegroundColor(displayColor);

                if (assemblyDisplayName != null)
                    Console.WriteLine($"   {assemblyDisplayName}: {args.Message.Message}");
                else
                    Console.WriteLine($"   {args.Message.Message}");

                if (!noColor)
                    ConsoleHelper.ResetColor();
            }
        }
    }
}
