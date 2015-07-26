using System.Threading.Tasks;

namespace Xunit.Sdk
{
    class AsyncManualResetEvent
    {
        volatile TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

        public AsyncManualResetEvent(bool signaled = false)
        {
            if (signaled)
                taskCompletionSource.TrySetResult(true);
        }

        public bool IsSet
        {
            get { return taskCompletionSource.Task.IsCompleted; }
        }

        public Task WaitAsync()
        {
            return taskCompletionSource.Task;
        }

        public void Set()
        {
            taskCompletionSource.TrySetResult(true);
        }

        public void Reset()
        {
            if (IsSet)
                taskCompletionSource = new TaskCompletionSource<bool>();
        }
    }
}
