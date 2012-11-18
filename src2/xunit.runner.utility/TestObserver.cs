using System;
using System.Security;
using Xunit.Abstractions;

namespace Xunit
{
    // TODO: Delete me (in favor of IMessageSink)
    public class TestObserver<T> : MarshalByRefObject, ITestObserver<T>
    {
        public Action Completed { get; set; }
        public Action<Exception> Error { get; set; }
        public Action<T> Next { get; set; }

        public void OnCompleted()
        {
            if (Completed != null)
                Completed();
        }

        public void OnError(Exception error)
        {
            if (Error != null)
                Error(error);
        }

        public void OnNext(T value)
        {
            if (Next != null)
                Next(value);
        }

        [SecurityCritical]
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
