using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    /// <summary>
    /// Notifies one or more waiting awaiters that an event has occurred
    /// </summary>
    [DebuggerDisplay("Signaled: {IsSet}")]
    internal class AsyncManualResetEvent
    {
        private volatile TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
        private readonly bool allowInliningAwaiters;
        private static readonly Task completedTask = Task.FromResult(true);

        public AsyncManualResetEvent(bool signaled = false)
        {
            if (signaled)
            {
                taskCompletionSource.TrySetResult(true);
            }
        }

        public bool IsSet
        {
            get { return taskCompletionSource.Task.IsCompleted; }
        }


        /// <summary>
        /// Returns a task that will be completed when this event is set.
        /// 
        /// </summary>
        public Task WaitAsync()
        {
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Sets this event to unblock callers of <see cref="M:AsyncManualResetEvent.WaitAsync"/>.
        /// 
        /// </summary>
        /// 
        /// <remarks>
        /// This method may return before the signal set has propagated (so <see cref="P:AsyncManualResetEvent.IsSet"/> may return <c>false</c> for a bit more if called immediately).
        ///             The returned task completes when the signal has definitely been set.
        /// 
        /// </remarks>
        public Task SetAsync()
        {
            var tcs = taskCompletionSource;
            if (allowInliningAwaiters)
            {
                tcs.TrySetResult(true);
            }
            else
            {
                Task.Factory.StartNew(
                    s => ((TaskCompletionSource<bool>)s).TrySetResult(true),
                    tcs,
                    CancellationToken.None,
                    TaskCreationOptions.PreferFairness,
                    TaskScheduler.Default);
            }
            return tcs.Task;
        }

        /// <summary>
        /// Sets the state of the event to nonsignaled, causing awaiters to block.
        /// </summary>
        public void Reset()
        {
            TaskCompletionSource<bool> tcs;
            do
            {
                tcs = taskCompletionSource;
            } while (tcs.Task.IsCompleted && Interlocked.CompareExchange(ref taskCompletionSource, new TaskCompletionSource<bool>(), tcs) != tcs);

        }

        public Task PulseAllAsync()
        {
            var setTask = SetAsync();
            if (setTask.IsCompleted)
            {
                Reset();
                return completedTask;
            }
            else
            {
                return setTask.ContinueWith(
                    (prev, s) => ((AsyncManualResetEvent)s).Reset(), 
                    this, 
                    CancellationToken.None, 
                    TaskContinuationOptions.None, 
                    TaskScheduler.Default);
            }
        }

        /// <summary>
        /// Gets an awaiter that completes when this event is signaled.
        /// 
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TaskAwaiter GetAwaiter()
        {
            return this.WaitAsync().GetAwaiter();
        }
    }
}
