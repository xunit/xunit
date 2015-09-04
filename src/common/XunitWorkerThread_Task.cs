using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    class XunitWorkerThread
    {
        readonly ManualResetEvent finished = new ManualResetEvent(false);
        static readonly TaskFactory taskFactory = new TaskFactory();

        public XunitWorkerThread(Action threadProc)
        {
            QueueUserWorkItem(threadProc, finished);
        }

        public void Join()
        {
            finished.WaitOne();
        }

        public static void QueueUserWorkItem(Action backgroundTask, EventWaitHandle finished = null)
        {
            taskFactory.StartNew(_ =>
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
                                 new State { BackgroundTask = backgroundTask, Finished = finished },
                                 CancellationToken.None,
                                 TaskCreationOptions.LongRunning,
                                 TaskScheduler.Default);
        }

        class State
        {
            public Action BackgroundTask;
            public EventWaitHandle Finished;
        }
    }
}