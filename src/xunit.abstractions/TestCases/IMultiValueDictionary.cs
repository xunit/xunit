using System;
using System.Collections.Generic;
using System.Linq;

namespace Xunit.Abstractions
{
    /// <summary>
    /// A collection of values which is stored as a dictionary, where one key can contain
    /// multiple values. Generally used to represent traits.
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface IMultiValueDictionary<TKey, TValue> : IEnumerable<TKey>
    {
        /// <summary>
        /// Gets the values for the given key.
        /// </summary>
        IEnumerable<TValue> this[TKey key] { get; }

        /// <summary>
        /// Gets the count of the keys in the dictionary.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        IEnumerable<TKey> Keys { get; }

        /// <summary>
        /// Adds the value for the given key. If the key does not exist in the
        /// dictionary yet, it will add it.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void Add(TKey key, TValue value);

        /// <summary>
        /// Removes all keys and values from the dictionary.
        /// </summary>
        void Clear();

        /// <summary>
        /// Determines whether the dictionary contains to specified key and value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        bool Contains(TKey key, TValue value);

        /// <summary>
        /// Calls the delegate once for each key/value pair in the dictionary.
        /// </summary>
        void ForEach(Action<TKey, TValue> code);

        /// <summary>
        /// Removes the given key and all of its values.
        /// </summary>
        void Remove(TKey key);

        /// <summary>
        /// Removes the given value from the given key. If this was the
        /// last value for the key, then the key is removed as well.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void RemoveValue(TKey key, TValue value);
    }
}
