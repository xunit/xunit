using System;

namespace Xunit.Abstractions
{
    // TODO: Delete me (in favor of IMessageSink)
    public interface ITestObserver<in T>
    {
        void OnCompleted();
        void OnError(Exception error);
        void OnNext(T value);
    }
}
