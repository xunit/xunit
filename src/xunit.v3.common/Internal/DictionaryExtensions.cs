using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class DictionaryExtensions
{
	/// <summary/>
	public static void Add<TKey, TValue>(
		this Dictionary<TKey, HashSet<TValue>> dictionary,
		TKey key,
		TValue value)
			where TKey : notnull
	{
		Guard.ArgumentNotNull(dictionary);

		dictionary.AddOrGet(key).Add(value);
	}

	/// <summary/>
	public static TValue AddOrGet<TKey, TValue>(
		this Dictionary<TKey, TValue> dictionary,
		TKey key)
			where TKey : notnull
			where TValue : new()
	{
		Guard.ArgumentNotNull(dictionary);

		return dictionary.AddOrGet(key, () => new TValue());
	}

	/// <summary/>
	public static TValue AddOrGet<TKey, TValue>(
		this Dictionary<TKey, TValue> dictionary,
		TKey key,
		Func<TValue> newValue)
			where TKey : notnull
	{
		Guard.ArgumentNotNull(dictionary);
		Guard.ArgumentNotNull(newValue);

		if (!dictionary.TryGetValue(key, out var result))
		{
			result = newValue();
			dictionary[key] = result;
		}

		return result;
	}

	/// <summary/>
	public static void AddRange<T>(
		this HashSet<T> hashSet,
		IEnumerable<T> values)
	{
		Guard.ArgumentNotNull(hashSet);
		Guard.ArgumentNotNull(values);

		foreach (var value in values)
			hashSet.Add(value);
	}

	/// <summary/>
	public static bool Contains<TKey, TValue>(
		this IReadOnlyDictionary<TKey, IReadOnlyCollection<TValue>> dictionary,
		TKey key,
		TValue value,
		IEqualityComparer<TValue> valueComparer)
			where TKey : notnull
	{
		Guard.ArgumentNotNull(dictionary);
		Guard.ArgumentNotNull(valueComparer);

		return dictionary.TryGetValue(key, out var values) && values.Contains(value, valueComparer);
	}

	/// <summary/>
	public static IReadOnlyDictionary<TKey, IReadOnlyCollection<TValue>> ToReadOnly<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> dictionary)
		where TKey : notnull =>
			Guard.ArgumentNotNull(dictionary).ToDictionary(
				kvp => kvp.Key,
				kvp => (IReadOnlyCollection<TValue>)kvp.Value,
				dictionary.Comparer
			);

	/// <summary/>
	public static IReadOnlyDictionary<TKey, IReadOnlyCollection<TValue>> ToReadOnly<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary)
		where TKey : notnull =>
			Guard.ArgumentNotNull(dictionary).ToDictionary(
				kvp => kvp.Key,
				kvp => (IReadOnlyCollection<TValue>)kvp.Value,
				dictionary.Comparer
			);

	/// <summary/>
	public static Dictionary<TKey, HashSet<TValue>> ToReadWrite<TKey, TValue>(
		this IReadOnlyDictionary<TKey, IReadOnlyCollection<TValue>> dictionary,
		IEqualityComparer<TKey>? comparer)
			where TKey : notnull =>
				Guard.ArgumentNotNull(dictionary).ToDictionary(
					kvp => kvp.Key,
					kvp => new HashSet<TValue>(kvp.Value),
					comparer
				);

	/// <summary/>
	public static void TryAdd<TKey, TValue>(
		this ConcurrentDictionary<TKey, ConcurrentBag<TValue>> dictionary,
		TKey key,
		TValue value)
			where TKey : notnull
	{
		Guard.ArgumentNotNull(dictionary);

		var bag = dictionary.GetOrAdd(key, _ => []);
		bag.Add(value);
	}
}
