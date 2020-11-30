using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
	public class SynchronousMessageBus : IMessageBus
	{
		readonly _IMessageSink messageSink;

		/// <summary/>
		public SynchronousMessageBus(_IMessageSink messageSink)
		{
			this.messageSink = Guard.ArgumentNotNull(nameof(messageSink), messageSink);
		}

		/// <summary/>
		public void Dispose()
		{ }

		/// <summary/>
		public bool QueueMessage(_MessageSinkMessage message)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			return messageSink.OnMessage(message);
		}
	}
}
