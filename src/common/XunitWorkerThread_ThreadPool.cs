using System;
using Windows.Foundation;
using Windows.System.Threading;

namespace Xunit.Sdk
{
    internal class XunitWorkerThread
    {
        readonly IAsyncAction reporterTask;

        public XunitWorkerThread(Action threadProc)
        {
            reporterTask = ThreadPool.RunAsync(_ => threadProc(), WorkItemPriority.Normal, WorkItemOptions.TimeSliced);
        }

        public void Join() { }
    }
}