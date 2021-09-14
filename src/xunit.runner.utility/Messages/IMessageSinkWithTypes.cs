using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Represents an endpoint for the reception of test messages. This endpoint can have the list of types
    /// of the message passed in to optimize the performance of message dispatching.
    /// </summary>
    public interface IMessageSinkWithTypes : IDisposable
    {
        /// <summary>
        /// Reports the presence of a message on the message bus with an optional list of message types.
        /// This method should never throw exceptions.
        /// </summary>
        /// <param name="message">The message from the message bus.</param>
        /// <param name="messageTypes">The list of message types, or <c>null</c>.</param>
        /// <returns>Return <c>true</c> to continue running tests, or <c>false</c> to stop.</returns>
        bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes);
    }
}
