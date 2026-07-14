using System.Security;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base class for scheduling parallel and sequential execution.
/// </summary>
public abstract class ExecutionScheduler : IAsyncDisposable
{
	bool disposed;
	readonly AsyncAutoResetEvent gate = new(initialState: true);
	int parallelCount;
	int sequentialCount;
	int? sequentialThreadId;

	internal static ExecutionScheduler Invalid =>
		_Invalid.Instance;

	/// <summary>
	/// Create the execution scheduler appropriate for the given parameters.
	/// </summary>
	/// <param name="maxParallelThreads">The maximum number of parallel tasks</param>
	/// <param name="parallelAlgorithm">The parallel algorithm used to limit the parallel tasks</param>
	/// <remarks>
	///	<para>The scheduler is chosen using the following rules:</para>
	///	<list type="bullet">
	///	<item>If <paramref name="maxParallelThreads"/> is less than <c>0</c>, use the unlimited scheduler</item>
	///	<item>If <paramref name="parallelAlgorithm"/> is <see cref="ParallelAlgorithm.Aggressive"/>, use the aggressive scheduler</item>
	///	<item>If <paramref name="parallelAlgorithm"/> is <see cref="ParallelAlgorithm.Conservative"/>, use the conservative scheduler</item>
	///	</list>
	/// <para>Passing <c>0</c> for <paramref name="maxParallelThreads"/> will use <see cref="Environment.ProcessorCount"/>.</para>
	/// </remarks>
	public static ExecutionScheduler Create(
		int maxParallelThreads,
		ParallelAlgorithm parallelAlgorithm) =>
			(maxParallelThreads, parallelAlgorithm) switch
			{
				( < 0, _) => CreateUnlimited(),
				(_, ParallelAlgorithm.Aggressive) => CreateAggressive(maxParallelThreads),
				(_, ParallelAlgorithm.Conservative) => CreateConservative(maxParallelThreads),
				_ => throw new ArgumentException($"Invalid parallel algorithm value {parallelAlgorithm}", nameof(parallelAlgorithm)),
			};

	/// <summary>
	/// Create an execution scheduler for the aggressive parallelism algorithm.
	/// </summary>
	/// <param name="maxParallelThreads">The maximum number of permitted parallel tasks</param>
	/// <remarks>
	/// Passing <c>0</c> for <paramref name="maxParallelThreads"/> will use <see cref="Environment.ProcessorCount"/>.
	/// </remarks>
	public static ExecutionScheduler CreateAggressive(int maxParallelThreads) =>
		new _Aggressive(maxParallelThreads > 0 ? maxParallelThreads : Environment.ProcessorCount);

	/// <summary>
	/// Create an execution scheduler for the conversative parallelism algorithm.
	/// </summary>
	/// <param name="maxParallelThreads">The maximum number of permitted parallel tasks</param>
	/// <remarks>
	/// Passing <c>0</c> for <paramref name="maxParallelThreads"/> will use <see cref="Environment.ProcessorCount"/>.
	/// </remarks>
	public static ExecutionScheduler CreateConservative(int maxParallelThreads) =>
		new _Conservative(maxParallelThreads > 0 ? maxParallelThreads : Environment.ProcessorCount);

	/// <summary>
	/// Create an execution scheduler for unlimited parallel tasks.
	/// </summary>
	public static ExecutionScheduler CreateUnlimited() =>
		new _Unlimited();

	/// <inheritdoc/>
	public virtual ValueTask DisposeAsync()
	{
		ThrowIfDisposed();
		disposed = true;

		GC.SuppressFinalize(this);

		return default;
	}

	/// <summary>
	/// Ensures that a new task starts in the background.
	/// </summary>
	/// <param name="taskFactory">The function that starts the task</param>
	/// <param name="cancellationToken">The cancellation token to run tasks with</param>
	/// <remarks>
	/// This uses either <see cref="Task.Run(Func{Task?}, CancellationToken)"/> or <see cref="TaskFactory{TResult}.StartNew(Func{TResult}, CancellationToken)"/>
	/// to ensure that the task starts immediately backgrounded. Typically tasks will run on the caller's thread and won't return
	/// until the first time an <c>await</c> statement would cause them to sleep.
	/// </remarks>
	public static async ValueTask<T> EnsureParallel<T>(
		Func<ValueTask<T>> taskFactory,
		CancellationToken cancellationToken)
	{
		if (SynchronizationContext.Current is not null)
		{
			var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
			return await Task.Factory.StartNew(() => taskFactory().AsTask(), cancellationToken, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, taskScheduler).Unwrap();
		}
		else
			return await Task.Run(() => taskFactory().AsTask(), cancellationToken);
	}

	async ValueTask<IDisposable> EnterParallelGate(CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			lock (gate)
				if (parallelCount > 0 || sequentialCount == 0)
				{
					++parallelCount;
					return new _UnlockParallel(this);
				}

			await gate.WaitAsync();
		}
	}

	async ValueTask<IDisposable> EnterSequentialGate(CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			lock (gate)
				if ((parallelCount == 0 && sequentialCount == 0) || sequentialThreadId == Environment.CurrentManagedThreadId)
				{
					++sequentialCount;
					sequentialThreadId = Environment.CurrentManagedThreadId;

					return new _UnlockSequential(this);
				}

			await gate.WaitAsync();
		}
	}

	/// <summary>
	/// Runs a task in parallel (that is, it can run in parallel with other tasks started via this function,
	/// but not against any other task started with <see cref="RunSequentialTask"/>.
	/// </summary>
	/// <param name="taskFactory">The parallel operation task</param>
	/// <param name="cancellationToken">The cancellation token to run tasks with</param>
	/// <remarks>
	/// <para>The specific implementation of this method will depend on the scheduler algorithm, and is used to
	/// limit the number of outstanding tasks. It should use <see cref="EnsureParallel"/> to guarantee that the
	/// parallel task is immediately launched into the background.</para>
	/// <para>This should only be invoked at the bottom level of parallelism; that is, in the test assembly runner when
	/// parallelizing test collections for <see cref="ParallelMode.Collections"/>, or in the test runner when parallelizing
	/// tests for <see cref="ParallelMode.All"/>.</para>
	/// </remarks>
	public abstract ValueTask<T> RunParallelTask<T>(
		Func<ValueTask<T>> taskFactory,
		CancellationToken cancellationToken);

	/// <summary>
	/// Runs a task in sequence (that is, it will not be run at the same time as any other parallel or sequential
	/// task).
	/// </summary>
	/// <param name="taskFactory">The sequential operation task</param>
	/// <param name="cancellationToken">The cancellation token to run tasks with</param>
	/// <remarks>
	/// This should only be invoked at the point where sequentialization has been requested; that is, anywhere
	/// that has been decorated with <c>DisableParallelization</c>, when the parallel mode is not <see cref="ParallelMode.None"/>.
	/// When code is running in <see cref="ParallelMode.None"/>, it is assumed that something <em>above this</em> in the stack
	/// has already put it into sequential mode (or the entire test assembly has disable parallelism, in which case there
	/// is no need to ever call this or <see cref="RunParallelTask"/>).
	/// </remarks>
	public async virtual ValueTask<T> RunSequentialTask<T>(
		Func<ValueTask<T>> taskFactory,
		CancellationToken cancellationToken)
	{
		using (await EnterSequentialGate(cancellationToken))
			return await Guard.ArgumentNotNull(taskFactory)();
	}

	/// <summary>
	/// Throws <see cref="ObjectDisposedException"/> if this class has already been disposed.
	/// </summary>
	protected void ThrowIfDisposed() =>
		ObjectDisposedException.ThrowIf(disposed, this);

	// Scheduler implementations

	sealed class _Aggressive(int maxParallelThreads) : ExecutionScheduler
	{
		readonly MaxConcurrencySyncContext syncContext = new(maxParallelThreads);

		/// <inheritdoc/>
		public override async ValueTask DisposeAsync()
		{
			await base.DisposeAsync();

			syncContext.SafeDispose();
		}

		/// <inheritdoc/>
		public override async ValueTask<T> RunParallelTask<T>(
			Func<ValueTask<T>> taskFactory,
			CancellationToken cancellationToken)
		{
			Guard.ArgumentNotNull(taskFactory);

			SetupSyncContextInternal(syncContext);

			using (await EnterParallelGate(cancellationToken))
				return await EnsureParallel(taskFactory, cancellationToken);
		}

		[SecuritySafeCritical]
		static void SetupSyncContextInternal(SynchronizationContext? context) =>
			SynchronizationContext.SetSynchronizationContext(context);

		/// <inheritdoc/>
		public override string ToString() =>
			$"Aggressive scheduler (max threads: {maxParallelThreads})";
	}

	sealed class _Conservative : ExecutionScheduler
	{
		readonly int maxParallelThreads;
		readonly SemaphoreSlim parallelSemaphore;

		public _Conservative(int maxParallelThreads)
		{
			this.maxParallelThreads = maxParallelThreads;

			parallelSemaphore = new(initialCount: maxParallelThreads);

			ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);
			var threadFloor = Math.Max(4, maxParallelThreads);
			if (workerThreads < threadFloor)
				ThreadPool.SetMinThreads(threadFloor, completionPortThreads);
		}

		public override async ValueTask DisposeAsync()
		{
			await base.DisposeAsync();

			parallelSemaphore.Dispose();
		}

		public override async ValueTask<T> RunParallelTask<T>(
			Func<ValueTask<T>> taskFactory,
			CancellationToken cancellationToken)
		{
			ThrowIfDisposed();
			Guard.ArgumentNotNull(taskFactory);

			await parallelSemaphore.WaitAsync(cancellationToken);

			try
			{
				using (await EnterParallelGate(cancellationToken))
					return await EnsureParallel(taskFactory, cancellationToken);
			}
			finally
			{
				parallelSemaphore.Release();
			}
		}

		public override string ToString() =>
			$"Conservative scheduler (max threads: {maxParallelThreads})";
	}

	sealed class _Invalid : ExecutionScheduler
	{
		public static _Invalid Instance { get; } = new();

		public override async ValueTask<T> RunParallelTask<T>(
			Func<ValueTask<T>> taskFactory,
			CancellationToken cancellationToken) =>
				throw new NotImplementedException("The InvalidExecutionScheduler should never be used to run tasks");

		public override ValueTask<T> RunSequentialTask<T>(
			Func<ValueTask<T>> taskFactory,
			CancellationToken cancellationToken) =>
				throw new NotImplementedException("The InvalidExecutionScheduler should never be used to run tasks");

		public override string ToString() =>
			"Invalid scheduler";
	}

	sealed class _Unlimited : ExecutionScheduler
	{
		public override async ValueTask<T> RunParallelTask<T>(
			Func<ValueTask<T>> taskFactory,
			CancellationToken cancellationToken)
		{
			using (await EnterParallelGate(cancellationToken))
				return await EnsureParallel(taskFactory, cancellationToken);
		}

		public override string ToString() =>
			"Unlimited scheduler";
	}

	sealed class _UnlockParallel(ExecutionScheduler scheduler) : IDisposable
	{
		public void Dispose()
		{
			int newCount;

			lock (scheduler.gate)
				newCount = --scheduler.parallelCount;

			if (newCount == 0)
				scheduler.gate.Set();
		}
	}

	sealed class _UnlockSequential(ExecutionScheduler scheduler) : IDisposable
	{
		public void Dispose()
		{
			int newCount;

			lock (scheduler.gate)
			{
				newCount = --scheduler.sequentialCount;

				if (newCount == 0)
					scheduler.sequentialThreadId = null;
			}

			if (newCount == 0)
				scheduler.gate.Set();
		}
	}
}
