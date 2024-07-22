using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents a handler for a message, which includes the ability to signal that
/// tests should stop running.
/// </summary>
/// <typeparam name="TMessage">The type of the message to be handled.</typeparam>
/// <param name="args">The message.</param>
public delegate void MessageHandler<TMessage>(MessageHandlerArgs<TMessage> args)
	where TMessage : IMessageSinkMessage;
