using System;
using System.Collections.Generic;
using System.Linq;

static class DictionaryExtensions
{
    public static void Add<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
    {
        dictionary.GetOrAdd(key).Add(value);
    }

    public static bool Contains<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value, IEqualityComparer<TValue> valueComparer)
    {
        List<TValue> values;

        if (!dictionary.TryGetValue(key, out values))
            return false;

        return values.Contains(value, valueComparer);
    }

    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        where TValue : new()
    {
        return dictionary.GetOrAdd<TKey, TValue>(key, () => new TValue());
    }

    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> newValue)
    {
        TValue result;

        if (!dictionary.TryGetValue(key, out result))
        {
            result = newValue();
            dictionary[key] = result;
        }

        return result;
    }

    // Uses the indexer instead of add, in case there are duplicate keys.
    public static Dictionary<TKey, TValue> ToDictionaryIgnoringDuplicateKeys<TKey, TValue>(this IEnumerable<TValue> values, Func<TValue, TKey> keySelector)
    {
        var result = new Dictionary<TKey, TValue>();

        foreach (var value in values)
        {
            var key = keySelector(value);
            if (!result.ContainsKey(key))
                result.Add(key, value);
        }

        return result;
    }
}
