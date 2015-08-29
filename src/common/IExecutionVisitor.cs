using System.Threading;
using Xunit.Abstractions;

namespace Xunit
{
    public interface IExecutionVisitor : IMessageSink
    {
        ExecutionSummary ExecutionSummary { get; }
        ManualResetEvent Finished { get; }
    }
}
