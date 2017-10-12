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

        public DiagnosticMessageSink(object consoleLock, string assemblyDisplayName, bool showDiagnostics, bool noColor, ConsoleColor displayColor = ConsoleColor.Yellow)
        {
            this.consoleLock = consoleLock;
            this.assemblyDisplayName = assemblyDisplayName;
            this.noColor = noColor;
            this.displayColor = displayColor;

            if (showDiagnostics)
                Diagnostics.DiagnosticMessageEvent += HandleDiagnosticMessage;
        }

        void HandleDiagnosticMessage(MessageHandlerArgs<IDiagnosticMessage> args)
        {
            lock (consoleLock)
            {
                if (!noColor)
                    ConsoleHelper.SetForegroundColor(displayColor);

                Console.WriteLine($"   {assemblyDisplayName}: {args.Message.Message}");

                if (!noColor)
                    ConsoleHelper.ResetColor();
            }
        }
    }
}
