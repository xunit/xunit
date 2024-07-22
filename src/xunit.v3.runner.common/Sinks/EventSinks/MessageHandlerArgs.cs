using Xunit.Sdk;

namespace Xunit.Runner.Common;

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
	public void Stop() => IsStopped = true;
}

/// <summary>
/// Wraps a message with the ability to cancel execution.
/// </summary>
/// <typeparam name="TMessage">The type of the message to be handled.</typeparam>
/// <param name="message">The message to be handled.</param>
public class MessageHandlerArgs<TMessage>(TMessage message) :
	MessageHandlerArgs
		where TMessage : IMessageSinkMessage
{
	/// <summary>
	/// Gets the message.
	/// </summary>
	public TMessage Message { get; } = message;
}
