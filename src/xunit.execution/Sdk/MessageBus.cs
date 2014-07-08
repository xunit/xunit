using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

#if WINDOWS_PHONE_APP
using Windows.Foundation;
using Windows.System.Threading;
#endif
namespace Xunit.Sdk
{
    /// <summary>
    /// This is an internal class, and is not intended to be called from end-user code.
    /// </summary>
    public class MessageBus : IMessageBus
    {
        volatile bool continueRunning = true;
        readonly IMessageSink messageSink;
        readonly ConcurrentQueue<IMessageSinkMessage> reporterQueue = new ConcurrentQueue<IMessageSinkMessage>();
#if !WINDOWS_PHONE_APP
        readonly Thread reporterThread;
#else
        readonly IAsyncAction reporterTask;
#endif
        readonly AutoResetEvent reporterWorkEvent = new AutoResetEvent(initialState: false);
        volatile bool shutdownRequested;

        /// <summary/>
        public MessageBus(IMessageSink messageSink)
        {
            this.messageSink = messageSink;

#if !WINDOWS_PHONE_APP
            reporterThread = new Thread(ReporterWorker);
            reporterThread.Start();
#else
            reporterTask = ThreadPool.RunAsync(_ => ReporterWorker(), WorkItemPriority.Normal, WorkItemOptions.TimeSliced);
#endif
        }

        private void DispatchMessages()
        {
            IMessageSinkMessage message;
            while (reporterQueue.TryDequeue(out message))
                try
                {
                    if (!messageSink.OnMessage(message))
                        continueRunning = false;
                }
                catch { }
        }
        /// <summary/>
        public void Dispose()
        {
            shutdownRequested = true;

            reporterWorkEvent.Set();
            
#if !WINDOWS_PHONE_APP
            reporterThread.Join();
#endif
            reporterWorkEvent.Dispose();
        }

        /// <summary/>
        public bool QueueMessage(IMessageSinkMessage message)
        {
            if (shutdownRequested)
                throw new ObjectDisposedException("MessageBus");

            reporterQueue.Enqueue(message);
            reporterWorkEvent.Set();
            return continueRunning;
        }

        void ReporterWorker()
        {
            while (!shutdownRequested)
            {
                reporterWorkEvent.WaitOne();
                DispatchMessages();
            }

            // One final dispatch pass
            DispatchMessages();
        }
    }
}
