using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Extension methods for <see cref="IMessageSinkMessage"/>.
/// </summary>
public static class MessageSinkMessageExtensions
{
    /// <summary>
    /// Attempts to optimally cast a message to the given message type, using the optional hash of
    /// interface types to improve casting performance.
    /// </summary>
    /// <typeparam name="TMessage">The desired destination message type.</typeparam>
    /// <param name="message">The message to test and cast.</param>
    /// <param name="typeNames">The implemented interfaces, if known.</param>
    /// <returns>The message as <typeparamref name="TMessage"/>, or <c>null</c>.</returns>
    public static TMessage Cast<TMessage>(this IMessageSinkMessage message, HashSet<string> typeNames)
            where TMessage : class, IMessageSinkMessage
        => typeNames == null || typeNames.Contains(typeof(TMessage).FullName) ? message as TMessage : null;

    /// <summary>
    /// Handles a message of a specific type by testing it for the type, as well as verifying that there
    /// is a registered callback;
    /// </summary>
    /// <param name="message">The message to dispatch.</param>
    /// <param name="messageTypes">The implemented interfaces, if known.</param>
    /// <param name="callback">The callback to dispatch the message to.</param>
    /// <returns>Returns <c>true</c> if processing should continue; <c>false</c> otherwise.</returns>
    public static bool Dispatch<TMessage>(this IMessageSinkMessage message, HashSet<string> messageTypes, MessageHandler<TMessage> callback)
        where TMessage : class, IMessageSinkMessage
    {
        if (callback != null)
        {
            var castMessage = Cast<TMessage>(message, messageTypes);
            if (castMessage != null)
            {
                var args = new MessageHandlerArgs<TMessage>(castMessage);
                callback(args);
                return !args.IsStopped;
            }
        }

        return true;
    }
}
