using System;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Sdk
{
	/// <summary>
	/// Implementation of <see cref="IMessageBus" /> that delegates to another implementation of
	/// <see cref="IMessageBus" /> while calling into a callback for each message.
	/// </summary>
	public class DelegatingMessageBus : IMessageBus
	{
		readonly Action<IMessageSinkMessage>? callback;
		readonly IMessageBus innerMessageBus;

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegatingMessageBus"/> class.
		/// </summary>
		/// <param name="innerMessageBus">The message bus to delegate to.</param>
		/// <param name="callback">The callback to send messages to.</param>
		public DelegatingMessageBus(
			IMessageBus innerMessageBus,
			Action<IMessageSinkMessage>? callback = null)
		{
			this.innerMessageBus = Guard.ArgumentNotNull(nameof(innerMessageBus), innerMessageBus);
			this.callback = callback;
		}

		/// <inheritdoc/>
		public virtual bool QueueMessage(IMessageSinkMessage message)
		{
			callback?.Invoke(message);

			return innerMessageBus.QueueMessage(message);
		}

		/// <inheritdoc/>
		public void Dispose()
		{ }
	}

	/// <summary>
	/// Implementation of <see cref="IMessageBus" /> that delegates to another implementation of
	/// <see cref="IMessageBus" /> while calling into a callback for each message. In addition,
	/// it issues a <see cref="Finished" /> event when a message of the type <typeparamref name="TFinalMessage"/>
	/// is seen.
	/// </summary>
	/// <typeparam name="TFinalMessage">The type of the T final message.</typeparam>
	public class DelegatingMessageBus<TFinalMessage> : DelegatingMessageBus
		where TFinalMessage : class, IMessageSinkMessage
	{
		TFinalMessage? finalMessage;

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegatingMessageSink{TFinalMessage}" /> class.
		/// </summary>
		/// <param name="innerMessageBus">The message bus to delegate to.</param>
		/// <param name="callback">The callback to send messages to.</param>
		public DelegatingMessageBus(
			IMessageBus innerMessageBus,
			Action<IMessageSinkMessage>? callback = null)
				: base(innerMessageBus, callback)
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
}
