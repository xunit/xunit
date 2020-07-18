#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

static class DictionaryExtensions
{
	public static void Add<TKey, TValue>(
		this IDictionary<TKey, List<TValue>> dictionary,
		TKey key,
		TValue value)
			where TKey : notnull
	{
		Guard.ArgumentNotNull(nameof(dictionary), dictionary);
		Guard.ArgumentNotNull(nameof(key), key);

		dictionary.GetOrAdd(key).Add(value);
	}

	public static bool Contains<TKey, TValue>(
		this IDictionary<TKey, List<TValue>> dictionary,
		TKey key,
		TValue value,
		IEqualityComparer<TValue> valueComparer)
			where TKey : notnull
	{
		Guard.ArgumentNotNull(nameof(dictionary), dictionary);
		Guard.ArgumentNotNull(nameof(key), key);
		Guard.ArgumentNotNull(nameof(valueComparer), valueComparer);

		if (!dictionary.TryGetValue(key, out var values))
			return false;

		return values.Contains(value, valueComparer);
	}

	public static TValue GetOrAdd<TKey, TValue>(
		this IDictionary<TKey, TValue> dictionary,
		TKey key)
			where TKey : notnull
			where TValue : new()
	{
		Guard.ArgumentNotNull(nameof(dictionary), dictionary);
		Guard.ArgumentNotNull(nameof(key), key);

		return dictionary.GetOrAdd(key, () => new TValue());
	}

	public static TValue GetOrAdd<TKey, TValue>(
		this IDictionary<TKey, TValue> dictionary,
		TKey key,
		Func<TValue> newValue)
			where TKey : notnull
	{
		Guard.ArgumentNotNull(nameof(dictionary), dictionary);
		Guard.ArgumentNotNull(nameof(key), key);
		Guard.ArgumentNotNull(nameof(newValue), newValue);

		if (!dictionary.TryGetValue(key, out var result))
		{
			result = newValue();
			dictionary[key] = result;
		}

		return result;
	}

	public static Dictionary<TKey, TValue> ToDictionaryIgnoringDuplicateKeys<TKey, TValue>(
		this IEnumerable<TValue> values,
		Func<TValue, TKey> keySelector,
		IEqualityComparer<TKey>? comparer = null)
			where TKey : notnull
		=> ToDictionaryIgnoringDuplicateKeys(values, keySelector, x => x, comparer);

	public static Dictionary<TKey, TValue> ToDictionaryIgnoringDuplicateKeys<TInput, TKey, TValue>(
		this IEnumerable<TInput> inputValues,
		Func<TInput, TKey> keySelector,
		Func<TInput, TValue> valueSelector,
		IEqualityComparer<TKey>? comparer = null)
			where TKey : notnull
	{
		Guard.ArgumentNotNull(nameof(inputValues), inputValues);
		Guard.ArgumentNotNull(nameof(keySelector), keySelector);
		Guard.ArgumentNotNull(nameof(valueSelector), valueSelector);

		var result = new Dictionary<TKey, TValue>(comparer);

		foreach (var inputValue in inputValues)
		{
			var key = keySelector(inputValue);
			if (!result.ContainsKey(key))
				result.Add(key, valueSelector(inputValue));
		}

		return result;
	}
}
