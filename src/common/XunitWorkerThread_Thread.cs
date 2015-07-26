using System;
using System.Threading;

namespace Xunit.Sdk
{
    class XunitWorkerThread
    {
        readonly Thread thread;

        public XunitWorkerThread(Action threadProc)
        {
            thread = new Thread(s => ((Action)s)()) { IsBackground = true };
            thread.Start(threadProc);
        }

        public void Join()
        {
            if (thread != Thread.CurrentThread)
                thread.Join();
        }

        public static void QueueUserWorkItem(Action backgroundTask)
        {
            ThreadPool.QueueUserWorkItem(s => ((Action)s)(), backgroundTask);
        }
    }
}
