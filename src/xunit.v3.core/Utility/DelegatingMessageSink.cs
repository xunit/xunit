using System;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="IMessageSink" /> that delegates to another implementation of
/// <see cref="IMessageSink" /> while calling into an optional callback for each message.
/// </summary>
/// <param name="innerSink">The inner message sink.</param>
/// <param name="callback">The callback.</param>
public class DelegatingMessageSink(
	IMessageSink innerSink,
	Action<IMessageSinkMessage>? callback = null) :
		IMessageSink
{
	readonly Action<IMessageSinkMessage>? callback = callback;
	readonly IMessageSink innerSink = Guard.ArgumentNotNull(innerSink);

	/// <inheritdoc/>
	public virtual bool OnMessage(IMessageSinkMessage message)
	{
		callback?.Invoke(message);

		return innerSink.OnMessage(message);
	}
}

/// <summary>
/// Implementation of <see cref="IMessageSink" /> that delegates to another implementation of
/// <see cref="IMessageSink" /> while calling into an optional callback for each message. In addition,
/// it issues a <see cref="Finished" /> event when a message of the type <typeparamref name="TFinalMessage"/>
/// is seen and records the final message for later retrieval.
/// </summary>
/// <typeparam name="TFinalMessage">The type of the T final message.</typeparam>
/// <param name="innerSink">The inner message sink.</param>
/// <param name="callback">The callback.</param>
public class DelegatingMessageSink<TFinalMessage>(
	IMessageSink innerSink,
	Action<IMessageSinkMessage>? callback = null) :
		DelegatingMessageSink(innerSink, callback)
			where TFinalMessage : IMessageSinkMessage
{
	TFinalMessage? finalMessage;

	/// <summary>
	/// The final message that was seen that caused <see cref="Finished"/> to be triggered.
	/// </summary>
	public TFinalMessage FinalMessage =>
		finalMessage ?? throw new InvalidOperationException("Attempted to retrieve FinalMessage before the final message has been seen.");

	/// <summary>
	/// An event that is triggered when a message of type <typeparamref name="TFinalMessage" /> is seen.
	/// </summary>
	public ManualResetEvent Finished { get; } = new ManualResetEvent(false);

	/// <inheritdoc/>
	public override bool OnMessage(IMessageSinkMessage message)
	{
		var result = base.OnMessage(message);

		if (message is TFinalMessage finalMessage)
		{
			this.finalMessage = finalMessage;
			Finished.Set();
		}

		return result;
	}
}
