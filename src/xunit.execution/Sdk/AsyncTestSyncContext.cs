using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Sdk
{
    internal class AsyncTestSyncContext : SynchronizationContext
    {
        readonly AsyncManualResetEvent @event = new AsyncManualResetEvent(true);
        Exception exception;
        int operationCount;

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

        public override async void Post(SendOrPostCallback d, object state)
        {
            // The call to Post() may be the state machine signaling that an exception is
            // about to be thrown, so we make sure the operation count gets incremented
            // before the Task.Run, and then decrement the count when the operation is done.
            OperationStarted();

            try
            {
                // await and eat exceptions that come from this post
                // We could get a thread abort, so we need to handle that
                await Task.Run(() =>
                {
                    try
                    {
                        Send(d, state);
                    }
                    finally
                    {
                        OperationCompleted();
                    }
                }).ConfigureAwait(false);
            }
            catch { }
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            try
            {
                d(state);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }

        public async Task<Exception> WaitForCompletionAsync()
        {
            await @event.WaitAsync().ConfigureAwait(false);

            return exception;
        }
    }
}