using System;
using Xunit.Abstractions;

namespace Xunit.ConsoleClient
{
    public class DiagnosticMessageSink : TestMessageSink
    {
        readonly string assemblyDisplayName;
        readonly object consoleLock;
        readonly bool noColor;

        public DiagnosticMessageSink(object consoleLock, string assemblyDisplayName, bool showDiagnostics, bool noColor)
        {
            this.consoleLock = consoleLock;
            this.assemblyDisplayName = assemblyDisplayName;
            this.noColor = noColor;

            if (showDiagnostics)
                DiagnosticMessageEvent += HandleDiagnosticMessage;
        }

        void HandleDiagnosticMessage(MessageHandlerArgs<IDiagnosticMessage> args)
        {
            lock (consoleLock)
            {
                if (!noColor)
                    Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine($"   {assemblyDisplayName}: {args.Message.Message}");

                if (!noColor)
                    Console.ResetColor();
            }
        }
    }
}
