#if NETSTANDARD1_1 || NETSTANDARD1_5 || WINDOWS_UAP

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    class XunitWorkerThread : IDisposable
    {
        readonly ManualResetEvent finished = new ManualResetEvent(false);
        static readonly TaskFactory taskFactory = new TaskFactory();

        public XunitWorkerThread(Action threadProc)
        {
            QueueUserWorkItem(threadProc, finished);
        }

        public void Dispose()
        {
            finished.Dispose();
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

#else

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

#endif
