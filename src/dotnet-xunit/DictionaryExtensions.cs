using System;
using System.Collections.Generic;

static class DictionaryExtensions
{
    public static string GetAndRemoveParameterWithValue(this Dictionary<string, string> dictionary, string key)
    {
        if (dictionary.TryGetValue(key, out var result))
        {
            if (result == null)
                throw new ArgumentException($"Missing value for option '{key}'");

            dictionary.Remove(key);
        }

        return result;
    }

    public static bool TryGetAndRemoveParameterWithoutValue(this Dictionary<string, string> dictionary, string key)
    {
        if (dictionary.TryGetValue(key, out var result))
        {
            if (result != null)
                throw new ArgumentException($"Option '{key}' should not have a value");

            dictionary.Remove(key);
            return true;
        }

        return false;
    }

    public static bool TryGetParameterWithoutValue(this Dictionary<string, string> dictionary, string key)
    {
        if (dictionary.TryGetValue(key, out var result))
        {
            if (result != null)
                throw new ArgumentException($"Option '{key}' should not have a value");

            return true;
        }

        return false;
    }
}
