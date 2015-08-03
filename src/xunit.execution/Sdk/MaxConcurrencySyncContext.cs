// HACK: This needs to be temporarily removed from DOTNETCORE because it uses ExecutionContext,
// which is a type that's not available for dnx451.
#if !DOTNETCORE

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
        bool disposed = false;
        readonly ManualResetEvent terminate = new ManualResetEvent(false);
        readonly List<XunitWorkerThread> workerThreads;
        readonly ConcurrentQueue<Tuple<SendOrPostCallback, object, ExecutionContext>> workQueue = new ConcurrentQueue<Tuple<SendOrPostCallback, object, ExecutionContext>>();
        readonly AutoResetEvent workReady = new AutoResetEvent(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxConcurrencySyncContext"/> class.
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
            if (disposed)
                return;

            disposed = true;
            terminate.Set();

            foreach (var workerThread in workerThreads)
                workerThread.Join();

            terminate.Dispose();
            workReady.Dispose();
        }

        /// <inheritdoc/>
        public override void Post(SendOrPostCallback d, object state)
        {
            // HACK: DNX on Unix seems to be calling this after it's disposed. In that case,
            // we'll just execute the code directly, which is a violation of the contract
            // but should be safe in this situation.
            if (disposed)
                Send(d, state);
            else
            {
                var context = ExecutionContext.Capture();
                workQueue.Enqueue(Tuple.Create(d, state, context));
                workReady.Set();
            }
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
                    if (work.Item3 == null)    // Fix for #461, so we don't try to run on a null execution context
                        RunOnSyncContext(work.Item1, work.Item2);
                    else
                        ExecutionContext.Run(work.Item3, _ => RunOnSyncContext(work.Item1, work.Item2), null);
            }
        }

        [SecuritySafeCritical]
        void RunOnSyncContext(SendOrPostCallback callback, object state)
        {
            var oldSyncContext = Current;
            SetSynchronizationContext(this);
            callback(state);
            SetSynchronizationContext(oldSyncContext);
        }
    }
}

#endif