using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Xunit.Sdk;

class AsyncManualResetEvent : IValueTaskSource
{
	volatile bool signaled = false;
	readonly ConcurrentQueue<Action> continuations = new();

	public AsyncManualResetEvent(bool signaled = false)
	{
		this.signaled = signaled;
	}

	public bool IsSet =>
		signaled;

	void IValueTaskSource.GetResult(short token)
	{ }

	ValueTaskSourceStatus IValueTaskSource.GetStatus(short token) =>
		signaled ? ValueTaskSourceStatus.Succeeded : ValueTaskSourceStatus.Pending;

	void NotifyAll()
	{
		while (continuations.TryDequeue(out var continuation))
			continuation();
	}

	void IValueTaskSource.OnCompleted(
		Action<object?> continuation,
		object? state,
		short token,
		ValueTaskSourceOnCompletedFlags flags)
	{
		var next = () =>
		{
			continuation(state);
		};

		if ((flags & ValueTaskSourceOnCompletedFlags.FlowExecutionContext) != 0)
		{
			var executionContext = ExecutionContext.Capture();
			if (executionContext != null)
			{
				var executionNext = next;
				next = () => ExecutionContext.Run(executionContext, s => ((Action)s!).Invoke(), executionNext);
			}
		}

		if ((flags & ValueTaskSourceOnCompletedFlags.UseSchedulingContext) != 0)
		{
			var syncContext = SynchronizationContext.Current;
			if (syncContext != null && syncContext.GetType() != typeof(SynchronizationContext))
			{
				var syncContextNext = next;
				next = () => syncContext.Post(s => ((Action)s!).Invoke(), syncContextNext);
			}
			else
			{
				var taskScheduler = TaskScheduler.Current;

				if (taskScheduler != TaskScheduler.Default)
				{
					var taskSchedulerNext = next;
					next = () => Task.Factory.StartNew(s => ((Action)s!).Invoke(), taskSchedulerNext, CancellationToken.None, TaskCreationOptions.DenyChildAttach, taskScheduler);
				}
			}
		}

		continuations.Enqueue(next);

		if (signaled)
			NotifyAll();
	}

	public void Reset() =>
		signaled = false;

	public void Set()
	{
		signaled = true;
		NotifyAll();
	}

	public ValueTask WaitAsync() =>
		new(this, 0);
}
