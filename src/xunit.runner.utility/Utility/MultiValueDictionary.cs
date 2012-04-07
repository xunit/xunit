using System.Collections.Generic;

namespace Xunit
{
    /// <summary>
    /// A dictionary which contains multiple unique values for each key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class MultiValueDictionary<TKey, TValue>
    {
        Dictionary<TKey, List<TValue>> dictionary = new Dictionary<TKey, List<TValue>>();

        /// <summary>
        /// Gets the values for the given key.
        /// </summary>
        public IEnumerable<TValue> this[TKey key]
        {
            get { return dictionary[key]; }
        }

        /// <summary>
        /// Gets the count of the keys in the dictionary.
        /// </summary>
        public int Count
        {
            get { return dictionary.Keys.Count; }
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        public IEnumerable<TKey> Keys
        {
            get { return dictionary.Keys; }
        }

        /// <summary>
        /// Adds the value for the given key. If the key does not exist in the
        /// dictionary yet, it will add it.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void AddValue(TKey key, TValue value)
        {
            List<TValue> items;

            if (!dictionary.TryGetValue(key, out items))
            {
                items = new List<TValue>();
                dictionary[key] = items;
            }

            if (!items.Contains(value))
                items.Add(value);
        }

        /// <summary>
        /// Removes all keys and values from the dictionary.
        /// </summary>
        public void Clear()
        {
            dictionary.Clear();
        }

        /// <summary>
        /// Determines whether the dictionary contains to specified key and value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public bool Contains(TKey key, TValue value)
        {
            List<TValue> items;

            if (!dictionary.TryGetValue(key, out items))
                return false;

            return items.Contains(value);
        }

        /// <summary/>
        public delegate void ForEachDelegate(TKey key, TValue value);

        /// <summary>
        /// Calls the delegate once for each key/value pair in the dictionary.
        /// </summary>
        public void ForEach(ForEachDelegate code)
        {
            foreach (TKey key in Keys)
                foreach (TValue value in this[key])
                    code(key, value);
        }

        /// <summary>
        /// Removes the given key and all of its values.
        /// </summary>
        public void Remove(TKey key)
        {
            dictionary.Remove(key);
        }

        /// <summary>
        /// Removes the given value from the given key. If this was the
        /// last value for the key, then the key is removed as well.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void RemoveValue(TKey key, TValue value)
        {
            List<TValue> items;

            if (!dictionary.TryGetValue(key, out items))
                return;

            items.Remove(value);

            if (items.Count == 0)
                dictionary.Remove(key);
        }
    }
}