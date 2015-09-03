using System;
using System.Threading;

namespace Xunit.Sdk
{
    class XunitWorkerThread : IDisposable
    {
        readonly Thread thread;

        public XunitWorkerThread(Action threadProc)
        {
            thread = new Thread(s => ((Action)s)()) { IsBackground = true };
            thread.Start(threadProc);
        }

        public void Dispose() { }

        public void Join()
        {
            if (thread != Thread.CurrentThread)
                thread.Join();
        }

        public static void QueueUserWorkItem(Action backgroundTask, EventWaitHandle finished = null)
        {
            ThreadPool.QueueUserWorkItem(_ =>
                                         {
                                             var state = (State)_;
                                         
                                             try
                                             {
                                                 state.BackgroundTask();
                                             }
                                             finally
                                             {
                                                 if (state.Finished != null)
                                                     state.Finished.Set();
                                             }
                                         },
                                         new State { BackgroundTask = backgroundTask, Finished = finished });
        }

        class State
        {
            public Action BackgroundTask;
            public EventWaitHandle Finished;
        }
    }
}
