using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Runner.TdNet
{
    public class MessageCollector<TFinalMessage> : LongLivedMarshalByRefObject, IMessageSink
    {
        public ManualResetEvent Finished = new ManualResetEvent(initialState: false);

        public List<ITestMessage> Messages = new List<ITestMessage>();

        public virtual void OnMessage(ITestMessage message)
        {
            Messages.Add(message);

            if (message is TFinalMessage)
                Finished.Set();
        }

        public void Dispose() { }
    }
}
