using System;

namespace Xunit.ConsoleClient
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            var consoleLock = new object();
            var commandLine = CommandLine.Parse(args);
            var internalDiagnosticsMessageSink = DiagnosticMessageSink.ForInternalDiagnostics(consoleLock, commandLine.InternalDiagnosticMessages, commandLine.NoColor);

            using (AssemblyHelper.SubscribeResolveForDirectory(internalDiagnosticsMessageSink))
                return new ConsoleRunner(consoleLock, commandLine).EntryPoint(args);
        }
    }
}
