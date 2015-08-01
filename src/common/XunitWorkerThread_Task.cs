using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    class XunitWorkerThread
    {
        static readonly TaskFactory taskFactory = new TaskFactory();

        public XunitWorkerThread(Action threadProc)
        {
            QueueUserWorkItem(threadProc);
        }

        public void Join() { }

        public static void QueueUserWorkItem(Action backgroundTask)
        {
            taskFactory.StartNew(backgroundTask,
                                 CancellationToken.None,
                                 TaskCreationOptions.LongRunning,
                                 TaskScheduler.Default);
        }
    }
}