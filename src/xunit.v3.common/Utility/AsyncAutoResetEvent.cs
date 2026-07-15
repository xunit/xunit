namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public class AsyncAutoResetEvent(bool initialState)
{
	bool signaled = initialState;
	readonly Stack<TaskCompletionSource<bool>> waitList = new();

	/// <summary/>
	public void Set()
	{
		var waiter = default(TaskCompletionSource<bool>);

		lock (waitList)
		{
			if (waitList.Count == 0)
			{
				signaled = true;
				return;
			}

			waiter = waitList.Pop();
		}

		waiter.TrySetResult(true);
	}

	/// <summary/>
	public Task WaitAsync()
	{
		lock (waitList)
		{
			if (signaled)
			{
				signaled = false;
				return Task.CompletedTask;
			}

			var waiter = new TaskCompletionSource<bool>();
			waitList.Push(waiter);
			return waiter.Task;
		}
	}
}
