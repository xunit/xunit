namespace Xunit.v3;

/// <summary>
/// Provides task runner utilities for test pipeline execution, handling synchronization context awareness.
/// </summary>
internal static class TestPipelineTaskRunner
{
	/// <summary>
	/// Creates a task runner function that properly handles synchronization context for test pipeline execution.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
	/// <returns>
	/// A function that takes a delegate returning <see cref="ValueTask{RunSummary}"/> and returns a <see cref="ValueTask{RunSummary}"/>
	/// that executes on the appropriate scheduler based on the current synchronization context.
	/// </returns>
	public static Func<Func<ValueTask<RunSummary>>, ValueTask<RunSummary>> Create(CancellationToken cancellationToken)
	{
		if (SynchronizationContext.Current is not null)
		{
			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			return code => new(Task.Factory.StartNew(() => code().AsTask(), cancellationToken,
				TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap());
		}

		return code => new(Task.Run(() => code().AsTask(), cancellationToken));
	}
}
