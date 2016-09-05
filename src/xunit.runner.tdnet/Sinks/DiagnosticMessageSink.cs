using TestDriven.Framework;

namespace Xunit.Runner.TdNet
{
    public class DiagnosticMessageSink : TestMessageSink
    {
        public DiagnosticMessageSink(ITestListener listener, string assemblyDisplayName, bool showDiagnostics)
        {
            if (showDiagnostics)
                Diagnostics.DiagnosticMessageEvent += args => listener.WriteLine($"{assemblyDisplayName}: {args.Message.Message}", Category.Warning);
        }
    }
}
