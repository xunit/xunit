#if NET5_0_OR_GREATER

using System.Collections;
using System.Collections.Generic;

public class ReadOnlySet<T> : IReadOnlySet<T>
{
	readonly HashSet<T> hashSet;

	public ReadOnlySet(
		IEqualityComparer<T> comparer,
		params T[] items) =>
			hashSet = new HashSet<T>(items, comparer);

	public int Count => hashSet.Count;

	public bool Contains(T item) => hashSet.Contains(item);
	public IEnumerator<T> GetEnumerator() => hashSet.GetEnumerator();
	public bool IsProperSubsetOf(IEnumerable<T> other) => hashSet.IsProperSubsetOf(other);
	public bool IsProperSupersetOf(IEnumerable<T> other) => hashSet.IsProperSupersetOf(other);
	public bool IsSubsetOf(IEnumerable<T> other) => hashSet.IsSubsetOf(other);
	public bool IsSupersetOf(IEnumerable<T> other) => hashSet.IsSupersetOf(other);
	public bool Overlaps(IEnumerable<T> other) => hashSet.Overlaps(other);
	public bool SetEquals(IEnumerable<T> other) => hashSet.SetEquals(other);
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

#endif
