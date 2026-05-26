using Xunit;
using Xunit.Sdk;

public static class TestPipelineSemaphoreTests
{
	[Fact]
	public static async Task ThrowsWithInvalidTestContextStage()
	{
		var cancellationToken = TestContext.Current.CancellationToken;

		// Set up an invalid test pipeline stage in the current TestContext
		TestContext.SetForInitialization(
			diagnosticMessageSink: null,
			diagnosticMessages: false,
			internalDiagnosticMessages: false);

		using var semaphore = new TestPipelineSemaphore(maximumConcurrentTests: 1);

		Assert.Throws<InvalidOperationException>(() => semaphore.CurrentCount);

		await Assert.ThrowsAnyAsync<InvalidOperationException>(async () =>
			await semaphore.WaitAsync(cancellationToken));

		await Assert.ThrowsAnyAsync<InvalidOperationException>(async () =>
		{
			using var _ = await semaphore.LockAsync(cancellationToken);
		});
	}

	[Fact]
	public static async Task CurrentCountUpdatesWhenAcquiredAndReleased()
	{
		using var semaphore = new TestPipelineSemaphore(maximumConcurrentTests: 1);

		Assert.Equal(1, semaphore.CurrentCount);

		using (await semaphore.LockAsync(TestContext.Current.CancellationToken))
		{
			Assert.Equal(0, semaphore.CurrentCount);
		}

		Assert.Equal(1, semaphore.CurrentCount);

		await semaphore.WaitAsync(TestContext.Current.CancellationToken);
		try
		{
			Assert.Equal(0, semaphore.CurrentCount);
		}
		finally
		{
			Assert.Equal(0, semaphore.Release());
		}

		Assert.Equal(1, semaphore.CurrentCount);
	}

	[Fact]
	public static async Task MultipleDisposesOnlyReleaseSemaphoreOnce()
	{
		using var semaphore = new TestPipelineSemaphore(maximumConcurrentTests: 1);

		var semaphoreReleaser = await semaphore.LockAsync(TestContext.Current.CancellationToken);

		semaphoreReleaser.Dispose();
		semaphoreReleaser.Dispose();

		Assert.Equal(1, semaphore.CurrentCount);
	}

	[Fact]
	public static async Task PreventsAcquiringMoreThanMaximumConcurrentTests()
	{
		using var semaphore = new TestPipelineSemaphore(maximumConcurrentTests: 1);

		using (await semaphore.LockAsync(TestContext.Current.CancellationToken))
		{
			using var waitCts = new CancellationTokenSource(millisecondsDelay: 50);

			await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
				await semaphore.WaitAsync(waitCts.Token));

			using var lockCts = new CancellationTokenSource(millisecondsDelay: 50);

			await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
			{
				using var _ = await semaphore.LockAsync(waitCts.Token);
			});
		}
	}

	[Fact]
	public static async Task SemaphoreAcquisitionWorksAcrossStages()
	{
		TestPipelineStage[] stages =
		[
			TestPipelineStage.Unknown,
			TestPipelineStage.Initialization,
			TestPipelineStage.Discovery,
			TestPipelineStage.TestAssemblyExecution,
			TestPipelineStage.TestCollectionExecution,
			TestPipelineStage.TestClassExecution,
			TestPipelineStage.TestMethodExecution,
			TestPipelineStage.TestCaseExecution,
			TestPipelineStage.TestExecution,
		];

		using var semaphore = new TestPipelineSemaphore(maximumConcurrentTests: 1);

		await using var disposalTracker = new DisposalTracker();

		foreach (var stage in stages)
		{
			if (TestPipelineSemaphore.ParallelizedTestPipelineStages.Contains(stage))
			{
				Assert.Equal(1, semaphore.GetCurrentCount(stage));

				await semaphore.WaitAsync(stage, TestContext.Current.CancellationToken);
				try
				{
					Assert.Equal(0, semaphore.GetCurrentCount(stage));
				}
				finally
				{
					Assert.Equal(0, semaphore.Release(stage));
				}

				disposalTracker.Add(await semaphore.LockAsync(stage, TestContext.Current.CancellationToken));
				Assert.Equal(0, semaphore.GetCurrentCount(stage));
			}
			else
			{
				Assert.Throws<InvalidOperationException>(() => semaphore.GetCurrentCount(stage));

				await Assert.ThrowsAnyAsync<InvalidOperationException>(async () =>
					await semaphore.WaitAsync(stage, TestContext.Current.CancellationToken));

				await Assert.ThrowsAnyAsync<InvalidOperationException>(async () =>
				{
					using var _ = await semaphore.LockAsync(stage, TestContext.Current.CancellationToken);
				});
			}
		}
	}
}
