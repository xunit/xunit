using Xunit.Sdk;

namespace Xunit.Internal;

/// <inheritdoc />
public sealed class TestPipelineSemaphore : ITestPipelineSemaphore
{
	private readonly Dictionary<TestPipelineStage, SemaphoreSlim> semaphores;

	/// <summary>
	/// Stages of the test execution pipeline that should have a semaphore created to limit the maximum tasks
	/// running for that stage.
	/// </summary>
	public static readonly TestPipelineStage[] ParallelizedTestPipelineStages =
	[
		TestPipelineStage.TestAssemblyExecution, TestPipelineStage.TestCollectionExecution,
		TestPipelineStage.TestClassExecution, TestPipelineStage.TestMethodExecution,
		TestPipelineStage.TestCaseExecution, TestPipelineStage.TestExecution
	];

	/// <summary>
	/// Initializes a new instance of the <see cref="TestPipelineSemaphore"/> class.
	/// </summary>
	/// <param name="maximumConcurrentTests">The maximum number of tests which are allowed to run concurrently.</param>
	public TestPipelineSemaphore(int maximumConcurrentTests)
	{
		semaphores =
			ParallelizedTestPipelineStages.ToDictionary(stage => stage, _ => new SemaphoreSlim(maximumConcurrentTests));
	}

	/// <inheritdoc />
	/// <exception cref="InvalidOperationException">When called outside a valid test pipeline stage.</exception>
	public int CurrentCount => GetCurrentCount(TestContext.Current.PipelineStage);

	/// <inheritdoc />
	/// <exception cref="InvalidOperationException">When called outside a valid test pipeline stage.</exception>
	public int Release() => Release(TestContext.Current.PipelineStage);

	/// <summary>
	/// Exits the <see cref="ITestPipelineSemaphore"/> for the given stage once.
	/// </summary>
	/// <param name="stage">The <see cref="TestPipelineStage"/> to release the semaphore for.</param>
	/// <returns>The previous count of the <see cref="ITestPipelineSemaphore"/>.</returns>
	/// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="InvalidOperationException">When called with an invalid test pipeline stage.</exception>
	public int Release(TestPipelineStage stage) => GetSemaphoreForStage(stage).Release();

	/// <inheritdoc />
	/// <exception cref="InvalidOperationException">When called outside a valid test pipeline stage.</exception>
	public Task WaitAsync(CancellationToken cancellationToken) =>
		WaitAsync(TestContext.Current.PipelineStage, cancellationToken);

	/// <summary>
	/// Asynchronously waits to enter the <see cref="ITestPipelineSemaphore"/> for the given stage, while observing a
	/// <see cref="CancellationToken"/>.
	/// </summary>
	/// <returns>
	/// A task that will complete when the semaphore has been entered and return a value which releases the semaphore
	/// when disposed.
	/// </returns>
	/// <param name="stage">The <see cref="TestPipelineStage"/> to acquire the semaphore for.</param>
	/// <param name="cancellationToken">The <see cref="CancellationToken"/> token to observe.</param>
	/// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="InvalidOperationException">When called with an invalid test pipeline stage.</exception>
	public Task WaitAsync(TestPipelineStage stage, CancellationToken cancellationToken)
	{
		var semaphore = GetSemaphoreForStage(stage);
		return semaphore.WaitAsync(cancellationToken);
	}

	/// <summary>
	/// Gets the current count of the <see cref="ITestPipelineSemaphore"/> for the given stage.
	/// </summary>
	/// <param name="stage">The <see cref="TestPipelineStage"/> to get the semaphore count for.</param>
	/// <returns>The current count of the <see cref="ITestPipelineSemaphore"/>.</returns>
	/// <exception cref="InvalidOperationException">When called with an invalid test pipeline stage.</exception>
	public int GetCurrentCount(TestPipelineStage stage) => GetSemaphoreForStage(stage).CurrentCount;

	/// <inheritdoc />
	/// <exception cref="InvalidOperationException">When called outside a valid test pipeline stage.</exception>
	public Task<IDisposable> LockAsync(CancellationToken cancellationToken) =>
		LockAsync(TestContext.Current.PipelineStage, cancellationToken);

	/// <summary>
	/// Asynchronously waits to enter the <see cref="ITestPipelineSemaphore"/> for the given stage, while observing a
	/// <see cref="CancellationToken"/>.
	/// </summary>
	/// <returns>
	/// A task that will complete when the semaphore has been entered and return a value which releases the semaphore
	/// when disposed.
	/// </returns>
	/// <param name="stage">The <see cref="TestPipelineStage"/> to acquire the semaphore for.</param>
	/// <param name="cancellationToken">The <see cref="CancellationToken"/> token to observe.</param>
	/// <exception cref="ObjectDisposedException">The current instance has already been disposed.</exception>
	/// <exception cref="InvalidOperationException">When called with an invalid test pipeline stage.</exception>
	public async Task<IDisposable> LockAsync(TestPipelineStage stage, CancellationToken cancellationToken)
	{
		var semaphore = GetSemaphoreForStage(stage);
		await semaphore.WaitAsync(cancellationToken);
		return new SemaphoreReleaser(semaphore);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		foreach (var kv in semaphores)
		{
			kv.Value.SafeDispose();
		}
	}

	private SemaphoreSlim GetSemaphoreForStage(TestPipelineStage stage) =>
		semaphores.TryGetValue(stage, out var semaphore)
			? semaphore
			: throw new InvalidOperationException(
				$"{nameof(TestPipelineSemaphore)} used during an invalid test pipeline stage {stage}.");

	/// <summary>Class that releases a <see cref="SemaphoreSlim"/> when disposed.</summary>
	/// <param name="semaphore">The semaphore to release when disposed.</param>
	private sealed class SemaphoreReleaser(SemaphoreSlim? semaphore) : IDisposable
	{
		private int _disposed;

		/// <inheritdoc />
		public void Dispose()
		{
			if (Interlocked.Exchange(ref _disposed, 1) == 0)
			{
				semaphore?.Release();
			}
		}
	}
}
