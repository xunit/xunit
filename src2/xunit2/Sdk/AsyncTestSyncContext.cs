using System;
using System.Linq;
using System.Threading;

namespace Xunit.Sdk
{
    internal class AsyncTestSyncContext : SynchronizationContext
    {
        readonly ManualResetEvent @event = new ManualResetEvent(initialState: true);
        Exception exception = null;
        int operationCount = 0;

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
            // before the QUWI, and then decrement the count when the operation is done.
            OperationStarted();

            ThreadPool.QueueUserWorkItem(s =>
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

        public Exception WaitForCompletion()
        {
            @event.WaitOne();
            return exception;
        }
    }
}