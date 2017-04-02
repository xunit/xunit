using System.Collections.Generic;

static class DictionaryExtensions
{
    public static string TryGetValueAndRemove(this Dictionary<string, string> dictionary, string key)
    {
        if (dictionary.TryGetValue(key, out var result))
            dictionary.Remove(key);

        return result;
    }
}
