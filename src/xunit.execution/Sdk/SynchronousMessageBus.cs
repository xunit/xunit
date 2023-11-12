using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// This is an internal class, and is not intended to be called from end-user code.
    /// </summary>
    public class SynchronousMessageBus : IMessageBus
    {
        volatile bool continueRunning = true;
        readonly IMessageSink messageSink;
        readonly bool stopOnFail;

        /// <summary/>
        public SynchronousMessageBus(IMessageSink messageSink, bool stopOnFail = false)
        {
            this.messageSink = messageSink;
            this.stopOnFail = stopOnFail;
        }

        /// <summary/>
        public void Dispose() { }

        /// <summary/>
        public bool QueueMessage(IMessageSinkMessage message)
        {
            if (stopOnFail && message is ITestFailed)
                continueRunning = false;

            return messageSink.OnMessage(message) && continueRunning;
        }
    }
}
