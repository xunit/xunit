using System;

namespace Xunit.Abstractions
{
    public interface IMessageSink : IDisposable
    {
        /// <summary>
        /// Reports the presence of a message on the message bus. This method should
        /// never throw exceptions.
        /// </summary>
        /// <param name="message">The message from the message bus</param>
        void OnMessage(ITestMessage message);
    }
}
