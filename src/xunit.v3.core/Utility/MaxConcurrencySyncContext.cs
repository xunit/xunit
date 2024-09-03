using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="SynchronizationContext"/> which runs work on custom threads
/// rather than in the thread pool, and limits the number of in-flight actions.
/// </summary>
public class MaxConcurrencySyncContext : SynchronizationContext, IDisposable
{
	bool disposed;
	readonly ManualResetEvent terminate = new(initialState: false);
	readonly List<Thread> workerThreads;
	readonly ConcurrentQueue<(SendOrPostCallback callback, object? state, ExecutionContext? context)> workQueue = new();
	readonly AutoResetEvent workReady = new(initialState: false);

	/// <summary>
	/// Initializes a new instance of the <see cref="MaxConcurrencySyncContext"/> class.
	/// </summary>
	/// <param name="maximumConcurrencyLevel">The maximum number of tasks to run at any one time.</param>
	public MaxConcurrencySyncContext(int maximumConcurrencyLevel) =>
		workerThreads =
			Enumerable
				.Range(0, maximumConcurrencyLevel)
				.Select(_ => { var result = new Thread(WorkerThreadProc); result.Start(); return result; })
				.ToList();

	/// <inheritdoc/>
	public void Dispose()
	{
		if (disposed)
			return;

		disposed = true;

		GC.SuppressFinalize(this);

		terminate.Set();

		foreach (var workerThread in workerThreads)
			workerThread.Join();

		terminate.Dispose();
		workReady.Dispose();
	}

	/// <inheritdoc/>
	public override void Post(
		SendOrPostCallback d,
		object? state)
	{
		// HACK: DNX on Unix seems to be calling this after it's disposed. In that case,
		// we'll just execute the code directly, which is a violation of the contract
		// but should be safe in this situation.
		if (disposed)
			Send(d, state);
		else
		{
			var context = ExecutionContext.Capture();
			workQueue.Enqueue((d, state, context));
			workReady.Set();
		}
	}

	/// <inheritdoc/>
	public override void Send(
		SendOrPostCallback d,
		object? state)
	{
		Guard.ArgumentNotNull(d);

		d(state);
	}

	[SecuritySafeCritical]
	void WorkerThreadProc()
	{
		while (true)
		{
			if (WaitHandle.WaitAny([workReady, terminate]) == 1)
				return;

			while (workQueue.TryDequeue(out var work))
			{
				// Set workReady() to wake up other threads, since there might still be work on the queue (fixes #877)
				workReady.Set();
				if (work.context is null)    // Fix for #461, so we don't try to run on a null execution context
					RunOnSyncContext(work.callback, work.state);
				else
					ExecutionContext.Run(work.context, _ => RunOnSyncContext(work.callback, work.state), null);
			}
		}
	}

	[SecuritySafeCritical]
	void RunOnSyncContext(
		SendOrPostCallback callback,
		object? state)
	{
		var oldSyncContext = Current;
		SetSynchronizationContext(this);
		callback(state);
		SetSynchronizationContext(oldSyncContext);
	}
}
