using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security;

namespace Xunit.Abstractions
{
    /// <summary>
    /// A dictionary which contains multiple unique values for each key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "This is a reasonable shortening of 'multiple'.")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This is a dictionary, not a collection.")]
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This is a dictionary.")]
    [Serializable]
    public class MultiValueDictionary<TKey, TValue> : IMultiValueDictionary<TKey, TValue>, ISerializable
    {
        readonly IDictionary<TKey, List<TValue>> dictionary = new Dictionary<TKey, List<TValue>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiValueDictionary{TKey, TValue}"/> class.
        /// </summary>
        public MultiValueDictionary() { }

        /// <summary>
        /// Serialization constructor. Not intended to be called directly.
        /// </summary>
        protected MultiValueDictionary(SerializationInfo info, StreamingContext context)
        {
            dictionary = (IDictionary<TKey, List<TValue>>)info.GetValue("InnerDictionary", typeof(IDictionary<TKey, List<TValue>>));
        }

        /// <inheritdoc/>
        public IEnumerable<TValue> this[TKey key]
        {
            get { return dictionary[key]; }
        }

        /// <inheritdoc/>
        public int Count
        {
            get { return dictionary.Keys.Count; }
        }

        /// <inheritdoc/>
        public IEnumerable<TKey> Keys
        {
            get { return dictionary.Keys; }
        }

        /// <inheritdoc/>
        public void Add(TKey key, TValue value)
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

        /// <inheritdoc/>
        public void Clear()
        {
            dictionary.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(TKey key, TValue value)
        {
            List<TValue> items;

            if (!dictionary.TryGetValue(key, out items))
                return false;

            return items.Contains(value);
        }

        /// <inheritdoc/>
        public void ForEach(Action<TKey, TValue> code)
        {
            foreach (TKey key in Keys)
                foreach (TValue value in this[key])
                    code(key, value);
        }

        /// <inheritdoc/>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("InnerDictionary", dictionary);
        }

        /// <inheritdoc/>
        public void Remove(TKey key)
        {
            dictionary.Remove(key);
        }

        /// <inheritdoc/>
        public void RemoveValue(TKey key, TValue value)
        {
            List<TValue> items;

            if (!dictionary.TryGetValue(key, out items))
                return;

            items.Remove(value);

            if (items.Count == 0)
                dictionary.Remove(key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.Keys.GetEnumerator();
        }

        IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
        {
            return dictionary.Keys.GetEnumerator();
        }
    }
}
