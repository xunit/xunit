using System;
using Windows.System.Threading;

namespace Xunit.Sdk
{
    internal class XunitWorkerThread
    {
        public XunitWorkerThread(Action threadProc)
        {
            QueueUserWorkItem(threadProc);
        }

        public void Join() { }

        public static void QueueUserWorkItem(Action backgroundTask)
        {
            var unused = ThreadPool.RunAsync(_ => backgroundTask(), WorkItemPriority.Normal, WorkItemOptions.TimeSliced);
        }
    }
}