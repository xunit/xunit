using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Internal
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
			this.messageSink = Guard.ArgumentNotNull(messageSink);
		}

		/// <summary/>
		public void Dispose()
		{ }

		/// <summary/>
		public bool QueueMessage(_MessageSinkMessage message)
		{
			Guard.ArgumentNotNull(message);

			return messageSink.OnMessage(message);
		}
	}
}
