using System.Threading.Tasks;

namespace Xunit.Sdk
{
	class AsyncManualResetEvent
	{
		volatile TaskCompletionSource<bool> taskCompletionSource = new();

		public AsyncManualResetEvent(bool signaled = false)
		{
			if (signaled)
				taskCompletionSource.TrySetResult(true);
		}

		public bool IsSet => taskCompletionSource.Task.IsCompleted;

		public void Reset()
		{
			if (IsSet)
				taskCompletionSource = new TaskCompletionSource<bool>();
		}

		public void Set() => taskCompletionSource.TrySetResult(true);

		public Task WaitAsync() => taskCompletionSource.Task;
	}
}
