using System;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents an endpoint for the reception of test messages.
    /// </summary>
    public interface IMessageSink
    {
        /// <summary>
        /// Reports the presence of a message on the message bus. This method should
        /// never throw exceptions.
        /// </summary>
        /// <param name="message">The message from the message bus</param>
        /// <returns>Return <c>true</c> to continue running tests, or <c>false</c> to stop.</returns>
        bool OnMessage(IMessageSinkMessage message);
    }
}