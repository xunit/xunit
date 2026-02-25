public static class EnumerableExtensions
{
	public async static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> data)
	{
		foreach (var dataItem in data)
			yield return dataItem;
	}
}
