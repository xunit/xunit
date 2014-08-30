using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;

namespace Xunit.Sdk
{
    /// <summary>
    /// An implementation of <see cref="SynchronizationContext"/> which runs work on custom threads
    /// rather than in the thread pool, and limits the number of in-flight actions.
    /// </summary>
    public class MaxConcurrencySyncContext : SynchronizationContext, IDisposable
    {
        readonly ManualResetEvent terminate = new ManualResetEvent(false);
        readonly List<XunitWorkerThread> workerThreads;
        readonly ConcurrentQueue<Tuple<SendOrPostCallback, object, ExecutionContext>> workQueue = new ConcurrentQueue<Tuple<SendOrPostCallback, object, ExecutionContext>>();
        readonly AutoResetEvent workReady = new AutoResetEvent(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxConcurrencyTaskScheduler"/> class.
        /// </summary>
        /// <param name="maximumConcurrencyLevel">The maximum number of tasks to run at any one time.</param>
        public MaxConcurrencySyncContext(int maximumConcurrencyLevel)
        {
            workerThreads = Enumerable.Range(0, maximumConcurrencyLevel)
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
        public override void Post(SendOrPostCallback d, object state)
        {
            var context = ExecutionContext.Capture();
            workQueue.Enqueue(Tuple.Create(d, state, context));
            workReady.Set();
        }

        /// <inheritdoc/>
        public override void Send(SendOrPostCallback d, object state)
        {
            d(state);
        }

        [SecuritySafeCritical]
        void WorkerThreadProc()
        {
            while (true)
            {
                if (WaitHandle.WaitAny(new WaitHandle[] { workReady, terminate }) == 1)
                    return;

                Tuple<SendOrPostCallback, object, ExecutionContext> work;
                while (workQueue.TryDequeue(out work))
                    ExecutionContext.Run(work.Item3, _ =>
                    {
                        var oldSyncContext = SynchronizationContext.Current;
                        SynchronizationContext.SetSynchronizationContext(this);
                        work.Item1(_);
                        SynchronizationContext.SetSynchronizationContext(oldSyncContext);
                    }, work.Item2);
            }
        }
    }
}
