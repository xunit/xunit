using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Xunit.Sdk;

// IMPORTANT NOTE: This class does its best to observe the contract of ManualResetEvent, but it technically violates
// the contract of ValueTask by always handing out the same token to all callers of WaitAsync. A proper implementation
// would hand out unique tokens from WaitAsync, and then fail during the call to GetResult() for anybody trying
// to read the state of the same ValueTask twice. I haven't done that here, because I am the only consumer of
// this class, and this would just add a performance penalty I don't wish to incur.
//
// If you want to use this code in production somewhere, you should make sure to add the extra tracking that I have
// left out.
//
// A simple way to allow a small number of WaitAsync callers would be to use a bitmask to keep track of the
// tokens you've handed out but not yet seen results for:
//
//   - Keep a short "next ID" in the event object
//   - Set the bitmask during WaitAsync for the ID you just handed out (and increment it)
//   - Check and clear the bitmask during GetResult to ensure nobody gets the result twice
//
// You can never reuse an ID. Depending on how many callers you expect for WaitAsync, you could use a single
// unsigned long as the bitmask, and that'll allow you up to 64 calls to WaitAsync before they would permanently
// fail. If you need more than 64, you may need to get creative on how you wish to track the outstanding tokens,
// depending on how many you expect to be outstanding at any one time (a full array of unsigned longs to support
// the full 2^16 tokens would be more than 4K bytes (64-bit mask * 1024 masks, plus the overhead of the array
// itself).
//
// Also make sure you have a lock around all ID manipulation in WaitAsync and GetResult so that you preserve the
// thread safety of the class.

class AsyncManualResetEvent : IValueTaskSource
{
	volatile bool signaled;
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
			if (executionContext is not null)
			{
				var executionNext = next;
				next = () => ExecutionContext.Run(executionContext, s => ((Action)s!).Invoke(), executionNext);
			}
		}

		if ((flags & ValueTaskSourceOnCompletedFlags.UseSchedulingContext) != 0)
		{
			var syncContext = SynchronizationContext.Current;
			if (syncContext is not null && syncContext.GetType() != typeof(SynchronizationContext))
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
