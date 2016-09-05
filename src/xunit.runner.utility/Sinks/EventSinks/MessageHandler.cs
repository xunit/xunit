using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Represents a handler for a specific <see cref="IMessageSinkMessage"/>.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to be handled.</typeparam>
    /// <param name="args">The message.</param>
    public delegate void MessageHandler<TMessage>(MessageHandlerArgs<TMessage> args)
        where TMessage : class, IMessageSinkMessage;
}
