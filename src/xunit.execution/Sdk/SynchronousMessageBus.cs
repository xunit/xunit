using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// This is an internal class, and is not intended to be called from end-user code.
    /// </summary>
    public class SynchronousMessageBus : IMessageBus
    {
        private IMessageSink _messageSink;

        /// <summary/>
        public SynchronousMessageBus(IMessageSink messageSink)
        {
            _messageSink = messageSink;
        }

        public void Dispose()
        {
            
        }

        /// <summary/>
        public bool QueueMessage(IMessageSinkMessage message)
        {
            return _messageSink.OnMessage(message);
        }
    }
}
