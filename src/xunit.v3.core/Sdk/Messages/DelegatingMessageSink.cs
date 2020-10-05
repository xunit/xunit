using System;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Sdk
{
	/// <summary>
	/// Implementation of <see cref="IMessageSink" /> that delegates to another implementation of
	/// <see cref="IMessageSink" /> while calling into a callback for each message.
	/// </summary>
	public class DelegatingMessageSink : IMessageSink
	{
		readonly Action<IMessageSinkMessage>? callback;
		readonly IMessageSink innerSink;

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegatingMessageSink"/> class.
		/// </summary>
		/// <param name="innerSink">The inner message sink.</param>
		/// <param name="callback">The callback.</param>
		public DelegatingMessageSink(
			IMessageSink innerSink,
			Action<IMessageSinkMessage>? callback = null)
		{
			this.innerSink = Guard.ArgumentNotNull(nameof(innerSink), innerSink);
			this.callback = callback;
		}

		/// <inheritdoc/>
		public void Dispose()
		{ }

		/// <inheritdoc/>
		public virtual bool OnMessage(IMessageSinkMessage message)
		{
			callback?.Invoke(message);

			return innerSink.OnMessage(message);
		}
	}

	/// <summary>
	/// Implementation of <see cref="IMessageSink" /> that delegates to another implementation of
	/// <see cref="IMessageSink" /> while calling into a callback for each message. In addition,
	/// it issues a <see cref="Finished" /> event when a message of the type <typeparamref name="TFinalMessage"/>
	/// is seen.
	/// </summary>
	/// <typeparam name="TFinalMessage">The type of the T final message.</typeparam>
	public class DelegatingMessageSink<TFinalMessage> : DelegatingMessageSink
		where TFinalMessage : class, IMessageSinkMessage
	{
		TFinalMessage? finalMessage;

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegatingMessageSink" /> class.
		/// </summary>
		/// <param name="innerSink">The inner message sink.</param>
		/// <param name="callback">The callback.</param>
		public DelegatingMessageSink(
			IMessageSink innerSink,
			Action<IMessageSinkMessage>? callback = null)
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
}
