using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    /// <summary>
    /// This class limits concurrency for all Tasks that are started with this scheduler, and
    /// also uses the stopwatch from the dictionary based on the lookup key that is passed as
    /// the async state to the task during creation. CallContext data is used to flow the
    /// stopwatch lookup key throughout the process (when it's not present, it pulls the key
    /// from the Task's AsyncState).
    /// </summary>
    public class MaxConcurrencyTaskScheduler : TaskScheduler, IDisposable
    {
        readonly int maximumConcurrencyLevel;
        readonly ManualResetEvent terminate = new ManualResetEvent(false);
        readonly List<XunitWorkerThread> workerThreads;
        readonly ConcurrentQueue<Task> workQueue = new ConcurrentQueue<Task>();
        readonly AutoResetEvent workReady = new AutoResetEvent(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxConcurrencyTaskScheduler"/> class.
        /// </summary>
        /// <param name="maximumConcurrencyLevel">The maximum number of tasks to run at any one time.</param>
        public MaxConcurrencyTaskScheduler(int maximumConcurrencyLevel)
        {
            this.maximumConcurrencyLevel = maximumConcurrencyLevel;

            workerThreads = Enumerable.Range(0, this.maximumConcurrencyLevel)
                                      .Select(_ => new XunitWorkerThread(WorkerThreadProc))
                                      .ToList();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            terminate.Set();

            foreach (var workerThread in workerThreads)
                workerThread.Join();

            terminate.Dispose();
            workReady.Dispose();
        }

        /// <inheritdoc/>
        public override int MaximumConcurrencyLevel
        {
            get { return maximumConcurrencyLevel; }
        }

        /// <inheritdoc/>
        [SecurityCritical]
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return workQueue.ToArray();
        }

        /// <inheritdoc/>
        [SecurityCritical]
        protected override void QueueTask(Task task)
        {
            workQueue.Enqueue(task);
            workReady.Set();
        }

        /// <inheritdoc/>
        [SecurityCritical]
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        [SecuritySafeCritical]
        void WorkerThreadProc()
        {
            while (true)
            {
                if (WaitHandle.WaitAny(new WaitHandle[] { workReady, terminate }) == 1)
                    return;

                Task task;
                while (workQueue.TryDequeue(out task))
                    TryExecuteTask(task);
            }
        }
    }
}
