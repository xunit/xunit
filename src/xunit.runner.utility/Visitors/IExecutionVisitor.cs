using System;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Represents an implementation of <see cref="IMessageSink"/> that is specifically used
    /// during test execution. Provides access to the final execution summary, as well as
    /// an event which is triggered when execution is finished.
    /// </summary>
    [Obsolete("This interface has poor performance; please use IExecutionSink instead.")]
    public interface IExecutionVisitor : IMessageSink
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
