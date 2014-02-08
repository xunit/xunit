﻿using System;
﻿using System.Reflection;
﻿using System.Threading;

namespace Xunit.Sdk
{
    internal class AsyncTestSyncContext : SynchronizationContext, IDisposable
    {
	    private readonly MethodInfo _method;
	    readonly ManualResetEvent @event = new ManualResetEvent(initialState: true);
        Exception exception;
        int operationCount;

	    public AsyncTestSyncContext(MethodInfo method)
	    {
		    _method = method;
	    }

	    public void Dispose()
        {
            ((IDisposable)@event).Dispose();
        }

        public override void OperationCompleted()
        {
            var result = Interlocked.Decrement(ref operationCount);
	        if (result == 0)
	        {
		        try
		        {
			        @event.Set();
		        }
		        catch (Exception e)
		        {
			        var name = _method.Name;
			        if (_method.DeclaringType != null)
				        name = _method.DeclaringType.FullName + "." + name;
			        Console.Error.WriteLine("Error when trying to set operation in: {1}, probably a leaked task?\r\n{0}", e, name);
		        }
	        }
        }

        public override void OperationStarted()
        {
            Interlocked.Increment(ref operationCount);
	        try
	        {
		        @event.Reset();
	        }
	        catch (Exception e)
	        {
				var name = _method.Name;
				if (_method.DeclaringType != null)
					name = _method.DeclaringType.FullName + "." + name;
				Console.Error.WriteLine("Error when trying to reset operation in: {1}, probably a leaked task?\r\n{0}", e, name);
	        }
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
	        try
	        {
		        @event.WaitOne();
	        }
	        catch (Exception e)
			{
				var name = _method.Name;
				if (_method.DeclaringType != null)
					name = _method.DeclaringType.FullName + "." + name;
				Console.Error.WriteLine("Error when trying to wait on operation in: {1}, probably a leaked task?\r\n{0}", e, name);
			}
            return exception;
        }
    }
}
