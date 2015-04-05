using System;
using System.Threading;

namespace Xunit.Sdk
{
    internal class XunitWorkerThread
    {
        readonly Thread thread;

        public XunitWorkerThread(Action threadProc)
        {
            thread = new Thread(() => threadProc()) { IsBackground = true };
            thread.Start();
        }

        public void Join()
        {
            if (thread != Thread.CurrentThread)
                thread.Join();
        }

        public static void QueueUserWorkItem(Action backgroundTask)
        {
            ThreadPool.QueueUserWorkItem(_ => backgroundTask());
        }
    }
}
