using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// This is an internal interface, and is not intended to be called from end-user code.
    /// </summary>
    public interface IMessageBus : IDisposable
    {
        /// <summary/>
        bool QueueMessage(IMessageSinkMessage message);
    }
}
