using Xunit.Abstractions;

namespace Xunit.Runner.VisualStudio
{
    internal interface IVsDiscoveryVisitor : IMessageSink
    {
        int Finish();
    }
}
