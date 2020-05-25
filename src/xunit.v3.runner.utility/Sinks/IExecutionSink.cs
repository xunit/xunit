using System;
using System.Threading;

namespace Xunit
{
    /// <summary>
    /// Represents an <see cref="IMessageSinkWithTypes"/> that can also provide execution
    /// information like an <see cref="IExecutionVisitor"/>.
    /// </summary>
    public interface IExecutionSink : IMessageSinkWithTypes, IDisposable
    {
        /// <summary>
        /// Gets the final execution summary, once the execution is finished.
        /// </summary>
        ExecutionSummary ExecutionSummary { get; }

        /// <summary>
        /// Gets an event which is signaled once execution is finished.
        /// </summary>
        ManualResetEvent Finished { get; }
    }
}
