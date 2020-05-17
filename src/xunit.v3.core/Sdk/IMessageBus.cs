using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Used by discovery, execution, and extensibility code to send messages to the runner.
    /// </summary>
    public interface IMessageBus : IDisposable
    {
        /// <summary>
        /// Queues a message to be sent to the runner.
        /// </summary>
        /// <param name="message">The message to be sent to the runner</param>
        /// <returns>
        /// Returns <c>true</c> if discovery/execution should continue; <c>false</c>, otherwise.
        /// The return value may be safely ignored by components which are not directly responsible
        /// for discovery or execution, and this is intended to communicate to those sub-systems that
        /// that they should short circuit and stop their work as quickly as is reasonable.
        /// </returns>
        bool QueueMessage(IMessageSinkMessage message);
    }
}
