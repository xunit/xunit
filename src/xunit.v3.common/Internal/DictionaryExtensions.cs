#nullable enable  // This file is temporarily shared with xunit.v2.tests, which is not nullable-enabled

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Xunit.Internal
{
	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
	public static class DictionaryExtensions
	{
		/// <summary/>
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

		/// <summary/>
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

		/// <summary/>
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

		/// <summary/>
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

		/// <summary/>
		public static Dictionary<TKey, TValue> ToDictionaryIgnoringDuplicateKeys<TKey, TValue>(
			this IEnumerable<TValue> values,
			Func<TValue, TKey> keySelector,
			IEqualityComparer<TKey>? comparer = null)
				where TKey : notnull
					=> ToDictionaryIgnoringDuplicateKeys(values, keySelector, x => x, comparer);

		/// <summary/>
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

		/// <summary/>
		public static IReadOnlyDictionary<TKey, IReadOnlyList<TValue>> ToReadOnly<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary)
			where TKey : notnull
				=> new ReadOnlyDictionary<TKey, IReadOnlyList<TValue>>(dictionary.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<TValue>)kvp.Value.AsReadOnly()));
	}
}
