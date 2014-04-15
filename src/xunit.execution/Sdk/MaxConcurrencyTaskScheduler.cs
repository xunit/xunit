﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    // This class limits concurrency for all Tasks that are started with this scheduler, and
    // also uses the stopwatch from the dictionary based on the lookup key that is passed as
    // the async state to the task during creation. CallContext data is used to flow the
    // stopwatch lookup key throughout the process (when it's not present, it pulls the key
    // from the Task's AsyncState).
    public class MaxConcurrencyTaskScheduler : TaskScheduler, IDisposable
    {
        readonly int maximumConcurrencyLevel;
        readonly ManualResetEvent terminate = new ManualResetEvent(false);
        readonly List<Thread> workerThreads;
        readonly ConcurrentQueue<Task> workQueue = new ConcurrentQueue<Task>();
        readonly AutoResetEvent workReady = new AutoResetEvent(false);

        public MaxConcurrencyTaskScheduler(int maximumConcurrencyLevel)
        {
            this.maximumConcurrencyLevel = maximumConcurrencyLevel;

            workerThreads = Enumerable.Range(0, this.maximumConcurrencyLevel)
                                      .Select(_ => new Thread(WorkerThreadProc))
                                      .ToList();

            for (int idx = 0; idx < workerThreads.Count; idx++)
                workerThreads[idx].Start(idx);
        }

        public void Dispose()
        {
            terminate.Set();
            workerThreads.ForEach(t => t.Join());

            terminate.Dispose();
            workReady.Dispose();
        }

        public override int MaximumConcurrencyLevel
        {
            get { return maximumConcurrencyLevel; }
        }

        [SecurityCritical]
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            throw new NotImplementedException();
        }

        [SecurityCritical]
        protected override void QueueTask(Task task)
        {
            workQueue.Enqueue(task);
            workReady.Set();
        }

        [SecurityCritical]
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        [SecuritySafeCritical]
        void WorkerThreadProc(object state)
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
