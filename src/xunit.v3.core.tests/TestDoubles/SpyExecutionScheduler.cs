using Xunit.Sdk;

namespace Xunit.v3;

public class SpyExecutionScheduler : ExecutionScheduler
{
	public List<string> Operations = [];

	public override ValueTask<T> RunParallelTask<T>(
		Func<ValueTask<T>> taskFactory,
		CancellationToken cancellationToken)
	{
		Operations.Add($"RunParallelTask<{ArgumentFormatter.FormatTypeName(typeof(T))}>");

		return new(default(T)!);
	}

	public override ValueTask<T> RunSequentialTask<T>(
		Func<ValueTask<T>> taskFactory,
		CancellationToken cancellationToken)
	{
		Operations.Add($"RunSequentialTask<{ArgumentFormatter.FormatTypeName(typeof(T))}>");

		return new(default(T)!);
	}
}
