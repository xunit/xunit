using System;

namespace Xunit.Abstractions
{
    public interface ITestObserver<in T>
    {
        void OnCompleted();
        void OnError(Exception error);
        void OnNext(T value);
    }
}
