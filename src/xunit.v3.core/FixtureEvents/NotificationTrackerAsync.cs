namespace Xunit.v3;

internal sealed class NotificationTrackerAsync<T>(
	IEnumerable<T> collection,
	Func<T, ValueTask> up,
	Func<T, ValueTask> down,
	CancellationToken cancellationToken) :
		IAsyncDisposable
{
	readonly Stack<T> itemsRun = [];

	public async ValueTask DisposeAsync()
	{
		foreach (var item in itemsRun)
			await down(item);
	}

	public async ValueTask<ExceptionAggregator> Up()
	{
		var aggregator = new ExceptionAggregator();

		foreach (var item in collection)
		{
			if (cancellationToken.IsCancellationRequested)
				break;

			try
			{
				await up(item);
				itemsRun.Push(item);
			}
			catch (Exception ex)
			{
				aggregator.Add(ex);
				break;
			}
		}

		return aggregator;
	}
}
