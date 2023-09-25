using System;
using System.Threading;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Implementation of <see cref="_IMessageSink" /> that delegates to another implementation of
/// <see cref="_IMessageSink" /> while calling into a callback for each message.
/// </summary>
public class DelegatingMessageSink : _IMessageSink
{
	readonly Action<_MessageSinkMessage>? callback;
	readonly _IMessageSink innerSink;

	/// <summary>
	/// Initializes a new instance of the <see cref="DelegatingMessageSink"/> class.
	/// </summary>
	/// <param name="innerSink">The inner message sink.</param>
	/// <param name="callback">The callback.</param>
	public DelegatingMessageSink(
		_IMessageSink innerSink,
		Action<_MessageSinkMessage>? callback = null)
	{
		this.innerSink = Guard.ArgumentNotNull(innerSink);
		this.callback = callback;
	}

	/// <inheritdoc/>
	public virtual bool OnMessage(_MessageSinkMessage message)
	{
		callback?.Invoke(message);

		return innerSink.OnMessage(message);
	}
}

/// <summary>
/// Implementation of <see cref="_IMessageSink" /> that delegates to another implementation of
/// <see cref="_IMessageSink" /> while calling into a callback for each message. In addition,
/// it issues a <see cref="Finished" /> event when a message of the type <typeparamref name="TFinalMessage"/>
/// is seen.
/// </summary>
/// <typeparam name="TFinalMessage">The type of the T final message.</typeparam>
public class DelegatingMessageSink<TFinalMessage> : DelegatingMessageSink
	where TFinalMessage : _MessageSinkMessage
{
	TFinalMessage? finalMessage;

	/// <summary>
	/// Initializes a new instance of the <see cref="DelegatingMessageSink" /> class.
	/// </summary>
	/// <param name="innerSink">The inner message sink.</param>
	/// <param name="callback">The callback.</param>
	public DelegatingMessageSink(
		_IMessageSink innerSink,
		Action<_MessageSinkMessage>? callback = null)
			: base(innerSink, callback)
	{
		Finished = new ManualResetEvent(false);
	}

	/// <summary>
	/// The final message that was seen that caused <see cref="Finished"/> to be triggered.
	/// </summary>
	public TFinalMessage FinalMessage =>
		finalMessage ?? throw new InvalidOperationException("Attempted to retrieve FinalMessage before the final message has been seen.");

	/// <summary>
	/// An event that is triggered when a message of type <typeparamref name="TFinalMessage" /> is seen.
	/// </summary>
	public ManualResetEvent Finished { get; }

	/// <inheritdoc/>
	public override bool OnMessage(_MessageSinkMessage message)
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
