using System;
using System.Collections.Generic;

static class DictionaryExtensions
{
    public static bool GetAndRemoveParameterWithoutValue(this Dictionary<string, List<string>> dictionary, string key)
    {
        var result = TryGetParameterWithoutValue(dictionary, key);
        if (result)
            dictionary.Remove(key);

        return result;
    }

    public static string GetAndRemoveParameterWithValue(this Dictionary<string, List<string>> dictionary, string key)
    {
        if (dictionary.TryGetSingleValue(key, out var result))
        {
            if (result == null)
                throw new ArgumentException($"Missing value for option '{key}'");

            dictionary.Remove(key);
        }

        return result;
    }

    public static bool TryGetAndRemoveParameterWithoutValue(this Dictionary<string, List<string>> dictionary, string key)
    {
        var result = TryGetParameterWithoutValue(dictionary, key);
        if (result)
            dictionary.Remove(key);

        return result;
    }

    public static bool TryGetParameterWithoutValue(this Dictionary<string, List<string>> dictionary, string key)
    {
        if (dictionary.TryGetSingleValue(key, out var result))
        {
            if (result != null)
                throw new ArgumentException($"Option '{key}' should not have a value");

            return true;
        }

        return false;
    }

    public static bool TryGetSingleValue(this Dictionary<string, List<string>> dictionary, string key, out string value)
    {
        value = null;

        if (!dictionary.TryGetValue(key, out var values))
            return false;

        if (values.Count > 1)
            throw new ArgumentException($"Option '{key}' cannot be set more than once");

        value = values[0];
        return true;
    }
}
