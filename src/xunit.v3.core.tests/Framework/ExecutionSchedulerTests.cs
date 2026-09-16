using System.Security;
using Xunit;
using Xunit.v3;

[TestClass(DisableParallelization = true)]
public static class ExecutionSchedulerTests
{
	[Fact]
	public static async ValueTask ParallelGateAllowsMultipleEntrants()
	{
		await using var scheduler = ExecutionScheduler.CreateUnlimited();

		var threads = new List<Thread>();
		var startCount = 0;
		var finishTask = new TaskCompletionSource<bool>();
		var finishCount = 0;

		for (var i = 0; i < 10; ++i)
		{
			var thread = new Thread(worker);
			threads.Add(thread);
			thread.Start();
		}

		// Wait 60 seconds for all 10 to start
		var startWaitMax = DateTimeOffset.Now.AddSeconds(60);
		while (startCount != 10)
		{
			if (DateTimeOffset.Now > startWaitMax)
				throw new InvalidOperationException("All 10 threads did not start within 60 seconds");

			await Task.Yield();
		}

		// Trigger the task that will let them know to finish
		finishTask.SetResult(true);

		// Wait 60 seconds for all 10 to finish
		var finishWaitMax = DateTimeOffset.Now.AddSeconds(60);
		while (finishCount != 10)
		{
			if (DateTimeOffset.Now > finishWaitMax)
				throw new InvalidOperationException("All 10 threads did not finish within 60 seconds");

			await Task.Yield();
		}

		// Clean up the threads
		foreach (var thread in threads)
			if (!thread.Join(TimeSpan.FromSeconds(60)))
				throw new InvalidOperationException("Thread did not clean up within 60 seconds");

		async void worker()
		{
			await scheduler.RunParallelTask(async () =>
			{
				Interlocked.Increment(ref startCount);
				await finishTask.Task;
				Interlocked.Increment(ref finishCount);

				return 0;
			}, TestContext.Current.CancellationToken);
		}
	}

	[Fact]
	public static async ValueTask SequentialGateAllowsSingleEntrant()
	{
		await using var scheduler = ExecutionScheduler.CreateUnlimited();

		var messages = new List<string>();
		var finishCount = 0;

		// Force to a single thread to expose https://github.com/xunit/xunit/issues/3629
		using var syncContext = new MaxConcurrencySyncContext(1);

		async ValueTask<Task[]> startTasks()
		{
			setupSyncContext(syncContext);

			return Enumerable.Range(0, 10).Select(startTask).ToArray();

			Task startTask(int index) =>
				Task.Factory.StartNew(async () =>
				{
					await scheduler.RunSequentialTask(async () =>
					{
						messages.Add($"Start {index} on thread {Environment.CurrentManagedThreadId}");
						await Task.Delay(10);
						messages.Add($"Finish {index} on thread {Environment.CurrentManagedThreadId}");

						Interlocked.Increment(ref finishCount);

						return 0;
					}, TestContext.Current.CancellationToken);
				}, TestContext.Current.CancellationToken, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
		}

		// Wait 60 seconds for all 10 tasks to start & finish
		var timeout = Task.Delay(60_000, TestContext.Current.CancellationToken);
		var tasks = await startTasks();
		if (await Task.WhenAny(Task.WhenAll(tasks), timeout) == timeout)
			throw new InvalidOperationException("All 10 tasks did not finish within 60 seconds");

		while (finishCount < 10)
		{
			if (timeout.Status == TaskStatus.RanToCompletion)
				throw new InvalidOperationException("All 10 tasks did not finish within 60 seconds");

			await Task.Yield();
		}

		Assert.Equal(20, messages.Count);

		// We don't know which thread will go first, we only know that it'll be pairs of start/finish messages
		for (var i = 0; i < 10; ++i)
		{
			var startMessage = messages[i * 2];
			Assert.StartsWith("Start ", startMessage);

			var trailer = startMessage.Substring(6);
			var finishMessage = messages[i * 2 + 1];
			Assert.Equal($"Finish {trailer}", finishMessage);
		}

		[SecuritySafeCritical]
		static void setupSyncContext(SynchronizationContext? context) =>
			SynchronizationContext.SetSynchronizationContext(context);
	}

	// This test is explicit because it relies upon tight timing. It gets run in CI during the TestMTP target.
	[Fact(Explicit = true)]
	public static async ValueTask Aggressive_AllowsTasksToRunWhenOtherTasksAreSleeping()
	{
		await using var scheduler = ExecutionScheduler.CreateAggressive(1);
		var messages = new List<string>();
		var tasks = new[] { makeTask(0, 100), makeTask(1, 5), makeTask(2, 50) }.Select(tf => scheduler.RunParallelTask(tf, TestContext.Current.CancellationToken).AsTask());

		await Task.WhenAll(tasks);

		Assert.Collection(
			messages,
			msg => Assert.Equal("Started 0", msg),
			msg => Assert.Equal("Started 1", msg),
			msg => Assert.Equal("Started 2", msg),
			msg => Assert.Equal("Finished 1", msg),
			msg => Assert.Equal("Finished 2", msg),
			msg => Assert.Equal("Finished 0", msg)
		);

		Func<ValueTask<int>> makeTask(
			int index,
			int delay) =>
				async () =>
				{
					lock (messages)
						messages.Add($"Started {index}");

					await Task.Delay(delay);

					lock (messages)
						messages.Add($"Finished {index}");

					return 0;
				};
	}

	// This test is explicit because it relies upon tight timing. It gets run in CI during the TestMTP target.
	[Fact(Explicit = true)]
	public static async ValueTask Conservative_FinishesEachTaskBeforeRunningNextTaskWhenQueueIsFull()
	{
		await using var scheduler = ExecutionScheduler.CreateConservative(1);
		var messages = new List<string>();
		var tasks = new[] { makeTask(0, 100), makeTask(1, 5), makeTask(2, 50) }.Select(tf => scheduler.RunParallelTask(tf, TestContext.Current.CancellationToken).AsTask());

		await Task.WhenAll(tasks);

		Assert.Collection(
			messages,
			msg => Assert.Equal("Started 0", msg),
			msg => Assert.Equal("Finished 0", msg),
			msg => Assert.Equal("Started 1", msg),
			msg => Assert.Equal("Finished 1", msg),
			msg => Assert.Equal("Started 2", msg),
			msg => Assert.Equal("Finished 2", msg)
		);

		Func<ValueTask<int>> makeTask(
			int index,
			int delay) =>
				async () =>
				{
					lock (messages)
						messages.Add($"Started {index}");

					await Task.Delay(delay);

					lock (messages)
						messages.Add($"Finished {index}");

					return 0;
				};
	}
}
