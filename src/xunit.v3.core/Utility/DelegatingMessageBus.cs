using System;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="IMessageBus"/> that delegates to another implementation of
/// <see cref="IMessageBus"/> while calling into an optional callback for each message.
/// </summary>
/// <param name="innerMessageBus">The message bus to delegate to.</param>
/// <param name="callback">The callback to send messages to.</param>
public class DelegatingMessageBus(
	IMessageBus innerMessageBus,
	Action<IMessageSinkMessage>? callback = null) :
		IMessageBus
{
	readonly IMessageBus innerMessageBus = Guard.ArgumentNotNull(innerMessageBus);

	/// <inheritdoc/>
	public virtual bool QueueMessage(IMessageSinkMessage message)
	{
		callback?.Invoke(message);

		return innerMessageBus.QueueMessage(message);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		GC.SuppressFinalize(this);

		innerMessageBus.SafeDispose();
	}
}

/// <summary>
/// Implementation of <see cref="IMessageBus"/> that delegates to another implementation of
/// <see cref="IMessageBus"/> while calling into an optional callback for each message. In addition,
/// it issues a <see cref="Finished"/> event when a message of the type <typeparamref name="TFinalMessage"/>
/// is seen and records the final message for later retrieval.
/// </summary>
/// <typeparam name="TFinalMessage">The type of the T final message.</typeparam>
/// <param name="innerMessageBus">The message bus to delegate to.</param>
/// <param name="callback">The callback to send messages to.</param>
public class DelegatingMessageBus<TFinalMessage>(
	IMessageBus innerMessageBus,
	Action<IMessageSinkMessage>? callback = null) :
		DelegatingMessageBus(innerMessageBus, callback)
			where TFinalMessage : IMessageSinkMessage
{
	TFinalMessage? finalMessage;

	/// <summary>
	/// The final message that was seen that caused <see cref="Finished"/> to be triggered.
	/// </summary>
	public TFinalMessage FinalMessage =>
		finalMessage ?? throw new InvalidOperationException("Attempted to retrieve FinalMessage before the final message has been seen.");

	/// <summary>
	/// An event that is triggered when a message of type <typeparamref name="TFinalMessage"/> is seen.
	/// </summary>
	public ManualResetEvent Finished { get; } = new ManualResetEvent(false);

	/// <inheritdoc/>
	public override bool QueueMessage(IMessageSinkMessage message)
	{
		var result = base.QueueMessage(message);

		if (message is TFinalMessage finalMessage)
		{
			this.finalMessage = finalMessage;
			Finished.Set();
		}

		return result;
	}
}
