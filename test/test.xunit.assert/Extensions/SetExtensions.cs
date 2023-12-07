using System.Collections.Generic;

internal static class SetExtensions
{
	public static SortedSet<T> ToSortedSet<T>(this ISet<T> set, IComparer<T>? comparer = null) =>
		new SortedSet<T>(set, comparer);
}
