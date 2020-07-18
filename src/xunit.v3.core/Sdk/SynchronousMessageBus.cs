using Xunit.Abstractions;

namespace Xunit.Sdk
{
	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
	public class SynchronousMessageBus : IMessageBus
	{
		readonly IMessageSink messageSink;

		/// <summary/>
		public SynchronousMessageBus(IMessageSink messageSink)
		{
			this.messageSink = Guard.ArgumentNotNull(nameof(messageSink), messageSink);
		}

		/// <summary/>
		public void Dispose()
		{ }

		/// <summary/>
		public bool QueueMessage(IMessageSinkMessage message)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			return messageSink.OnMessage(message);
		}
	}
}
