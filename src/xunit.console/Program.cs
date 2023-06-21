using System;
using System.Linq;
using System.Text;

namespace Xunit.ConsoleClient
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            // This code must not contain any references to code in any external assembly (or any code that references any code in any
            // other assembly) until AFTER the creation of the AssemblyHelper.
            var consoleLock = new object();
            var internalDiagnosticsMessageSink = DiagnosticMessageSink.ForInternalDiagnostics(consoleLock, args.Contains("-internaldiagnostics"), args.Contains("-nocolor"));

            using (AssemblyHelper.SubscribeResolveForAssembly(typeof(Program), internalDiagnosticsMessageSink))
                return new ConsoleRunner(consoleLock).EntryPoint(args);
        }
    }
}
