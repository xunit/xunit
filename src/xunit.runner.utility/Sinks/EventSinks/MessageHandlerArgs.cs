using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Allows cancellation during message handling.
    /// </summary>
    public abstract class MessageHandlerArgs
    {
        /// <summary>
        /// Gets a value to indicate whether stop has been requested.
        /// </summary>
        public bool IsStopped { get; private set; }

        /// <summary>
        /// Call to indicate that execution should stop.
        /// </summary>
        public void Stop()
        {
            IsStopped = true;
        }
    }

    /// <summary>
    /// Wraps a specific <see cref="IMessageSinkMessage"/> with the ability to cancel execution.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to be handled.</typeparam>
    public class MessageHandlerArgs<TMessage> : MessageHandlerArgs
        where TMessage : class, IMessageSinkMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlerArgs{TMessage}"/> class.
        /// </summary>
        /// <param name="message">The message to be handled.</param>
        public MessageHandlerArgs(TMessage message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public TMessage Message { get; }
    }
}
