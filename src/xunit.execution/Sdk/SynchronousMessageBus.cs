using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// This is an internal class, and is not intended to be called from end-user code.
    /// </summary>
    public class SynchronousMessageBus : IMessageBus
    {
        readonly IMessageSink messageSink;

        /// <summary/>
        public SynchronousMessageBus(IMessageSink messageSink)
        {
            this.messageSink = messageSink;
        }

        /// <summary/>
        public void Dispose() { }

        /// <summary/>
        public bool QueueMessage(IMessageSinkMessage message)
        {
            return messageSink.OnMessage(message);
        }
    }
}
