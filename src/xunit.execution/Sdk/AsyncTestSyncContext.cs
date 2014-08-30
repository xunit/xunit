using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

#if WINDOWS_PHONE_APP
using Windows.System.Threading;
#endif

namespace Xunit.Sdk
{
    internal class AsyncTestSyncContext : SynchronizationContext
    {
        readonly AsyncManualResetEvent @event = new AsyncManualResetEvent(true);
        Exception exception;
        readonly SynchronizationContext innerContext;
        int operationCount;

        public AsyncTestSyncContext(SynchronizationContext innerContext)
        {
            this.innerContext = innerContext;
        }

        public override void OperationCompleted()
        {
            var result = Interlocked.Decrement(ref operationCount);
            if (result == 0)
                @event.Set();
        }

        public override void OperationStarted()
        {
            Interlocked.Increment(ref operationCount);
            @event.Reset();
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            // The call to Post() may be the state machine signaling that an exception is
            // about to be thrown, so we make sure the operation count gets incremented
            // before the Task.Run, and then decrement the count when the operation is done.
            OperationStarted();

            try
            {
                if (innerContext == null)
                {
                    XunitWorkerThread.QueueUserWorkItem(() =>
                    {
                        try
                        {
                            Send(d, state);
                        }
                        finally
                        {
                            OperationCompleted();
                        }
                    });
                }
                else
                    innerContext.Post(_ =>
                    {
                        try
                        {
                            Send(d, _);
                        }
                        finally
                        {
                            OperationCompleted();
                        }
                    }, state);
            }
            catch { }
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            try
            {
                if (innerContext != null)
                    innerContext.Send(d, state);
                else
                    d(state);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }

        public async Task<Exception> WaitForCompletionAsync()
        {
            await @event.WaitAsync();

            return exception;
        }
    }
}