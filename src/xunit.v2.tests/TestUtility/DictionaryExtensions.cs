using System;
using System.Collections.Generic;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
static class DictionaryExtensions
{
	/// <summary/>
	public static void Add<TKey, TValue>(
		this Dictionary<TKey, List<TValue>> dictionary,
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
}
