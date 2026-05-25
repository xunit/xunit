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
	public int CurrentCount => TestPipelineStageSemaphore.CurrentCount;

	private SemaphoreSlim TestPipelineStageSemaphore
	{
		get
		{
			var stage = GetTestPipelineStage();
			return semaphores[stage];
		}
	}

	/// <inheritdoc />
	/// <exception cref="InvalidOperationException">When called outside a valid test pipeline stage.</exception>
	public int Release() => TestPipelineStageSemaphore.Release();

	/// <inheritdoc />
	/// <exception cref="InvalidOperationException">When called outside a valid test pipeline stage.</exception>
	public async Task<ReleaseHandle> WaitAsync(CancellationToken cancellationToken)
	{
		var semaphore = TestPipelineStageSemaphore;
		await semaphore.WaitAsync(cancellationToken);
		return new ReleaseHandle(semaphore);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		foreach (var kv in semaphores)
		{
			kv.Value.SafeDispose();
		}
	}

	private TestPipelineStage GetTestPipelineStage()
	{
		var stage = TestContext.Current.PipelineStage;
		if (!semaphores.ContainsKey(stage))
			throw new InvalidOperationException(
				$"{nameof(TestPipelineSemaphore)} used during an invalid test pipeline stage {stage}.");

		return stage;
	}
}
