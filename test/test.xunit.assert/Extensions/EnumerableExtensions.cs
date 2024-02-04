#if NETCOREAPP3_0_OR_GREATER

using System.Collections.Generic;

public static class EnumerableExtensions
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
	public async static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> data)
	{
		foreach (var dataItem in data)
			yield return dataItem;
	}
#pragma warning restore CS1998
}

#endif
