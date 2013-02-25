using System;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class DelegatingMessageSink : IMessageSink
    {
        Action<ITestMessage> callback;
        IMessageSink innerSink;

        public DelegatingMessageSink(IMessageSink innerSink, Action<ITestMessage> callback = null)
        {
            this.innerSink = innerSink;
            this.callback = callback;
        }

        public virtual void OnMessage(ITestMessage message)
        {
            if (callback != null)
                callback(message);

            innerSink.OnMessage(message);
        }

        public void Dispose() { }
    }

    public class DelegatingMessageSink<TFinalMessage> : DelegatingMessageSink
        where TFinalMessage : class, ITestMessage
    {
        public DelegatingMessageSink(IMessageSink innerSink, Action<ITestMessage> callback = null)
            : base(innerSink, callback)
        {
            Finished = new ManualResetEvent(initialState: false);
        }

        public TFinalMessage FinalMessage { get; private set; }

        public ManualResetEvent Finished { get; private set; }

        public override void OnMessage(ITestMessage message)
        {
            base.OnMessage(message);

            TFinalMessage finalMessage = message as TFinalMessage;
            if (finalMessage != null)
            {
                FinalMessage = finalMessage;
                Finished.Set();
            }
        }
    }
}
