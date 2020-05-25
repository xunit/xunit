// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information. 

/*
 * WARNING: Auto-generated file (7/18/2012 4:59:53 PM)
 *
 * Stripped down code based on ndp\clr\src\BCL\System\Collections\Concurrent\ConcurrentDictionary.cs
 */

#if NET35

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

namespace System.Collections.Concurrent
{
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    internal class ConcurrentDictionary<TKey, TValue> 
    {
        private const int DEFAULT_CONCURRENCY_MULTIPLIER = 4;
        private const int DEFAULT_CAPACITY = 31;
        [NonSerialized]
        private volatile ConcurrentDictionary<TKey, TValue>.Node[] m_buckets;
        [NonSerialized]
        private object[] m_locks;
        [NonSerialized]
        private volatile int[] m_countPerLock;
        private IEqualityComparer<TKey> m_comparer;
        private KeyValuePair<TKey, TValue>[] m_serializationArray;
        private int m_serializationConcurrencyLevel;
        private int m_serializationCapacity;

        public ConcurrentDictionary()
          : this(ConcurrentDictionary<TKey, TValue>.DefaultConcurrencyLevel, 31)
        {
        }

        public ConcurrentDictionary(int concurrencyLevel, int capacity)
          : this(concurrencyLevel, capacity, (IEqualityComparer<TKey>)EqualityComparer<TKey>.Default)
        {
        }

        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
          : this(collection, (IEqualityComparer<TKey>)EqualityComparer<TKey>.Default)
        {
        }

        public ConcurrentDictionary(IEqualityComparer<TKey> comparer)
          : this(ConcurrentDictionary<TKey, TValue>.DefaultConcurrencyLevel, 31, comparer)
        {
        }

        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
          : this(ConcurrentDictionary<TKey, TValue>.DefaultConcurrencyLevel, collection, comparer)
        {
        }

        public ConcurrentDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
          : this(concurrencyLevel, 31, comparer)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            this.InitializeFromCollection(collection);
        }

        private void InitializeFromCollection(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            foreach (KeyValuePair<TKey, TValue> keyValuePair in collection)
            {
                if ((object)keyValuePair.Key == null)
                    throw new ArgumentNullException("key");
                TValue resultingValue;
                if (!this.TryAddInternal(keyValuePair.Key, keyValuePair.Value, false, false, out resultingValue))
                    throw new ArgumentException("SourceContainsDuplicateKeys");
            }
        }

        public ConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
        {
            if (concurrencyLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(concurrencyLevel), "ConcurrencyLevelMustBePositive");
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "CapacityMustNotBeNegative");
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            if (capacity < concurrencyLevel)
                capacity = concurrencyLevel;
            this.m_locks = new object[concurrencyLevel];
            for (int index = 0; index < this.m_locks.Length; ++index)
                this.m_locks[index] = new object();
            this.m_countPerLock = new int[this.m_locks.Length];
            this.m_buckets = new ConcurrentDictionary<TKey, TValue>.Node[capacity];
            this.m_comparer = comparer;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            if ((object)key == null)
                throw new ArgumentNullException(nameof(key));
            TValue resultingValue;
            return this.TryAddInternal(key, value, false, true, out resultingValue);
        }

        public bool ContainsKey(TKey key)
        {
            if ((object)key == null)
                throw new ArgumentNullException(nameof(key));
            TValue obj;
            return this.TryGetValue(key, out obj);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            if ((object)key == null)
                throw new ArgumentNullException(nameof(key));
            return this.TryRemoveInternal(key, out value, false, default(TValue));
        }

        private bool TryRemoveInternal(TKey key, out TValue value, bool matchValue, TValue oldValue)
        {
            label_0:
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_buckets;
            int bucketNo;
            int lockNo;
            this.GetBucketAndLockNo(this.m_comparer.GetHashCode(key), out bucketNo, out lockNo, buckets.Length);
            lock (this.m_locks[lockNo])
            {
                if (buckets == this.m_buckets)
                {
                    ConcurrentDictionary<TKey, TValue>.Node node1 = (ConcurrentDictionary<TKey, TValue>.Node)null;
                    for (ConcurrentDictionary<TKey, TValue>.Node node2 = this.m_buckets[bucketNo]; node2 != null; node2 = node2.m_next)
                    {
                        if (this.m_comparer.Equals(node2.m_key, key))
                        {
                            if (matchValue && !EqualityComparer<TValue>.Default.Equals(oldValue, node2.m_value))
                            {
                                value = default(TValue);
                                return false;
                            }
                            if (node1 == null)
                                this.m_buckets[bucketNo] = node2.m_next;
                            else
                                node1.m_next = node2.m_next;
                            value = node2.m_value;
                            --this.m_countPerLock[lockNo];
                            return true;
                        }
                        node1 = node2;
                    }
                }
                else
                    goto label_0;
            }
            value = default(TValue);
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if ((object)key == null)
                throw new ArgumentNullException(nameof(key));
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_buckets;
            int bucketNo;
            int lockNo;
            this.GetBucketAndLockNo(this.m_comparer.GetHashCode(key), out bucketNo, out lockNo, buckets.Length);
            ConcurrentDictionary<TKey, TValue>.Node next = buckets[bucketNo];
            Thread.MemoryBarrier();
            for (; next != null; next = next.m_next)
            {
                if (this.m_comparer.Equals(next.m_key, key))
                {
                    value = next.m_value;
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }

        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            if ((object)key == null)
                throw new ArgumentNullException(nameof(key));
            int hashCode = this.m_comparer.GetHashCode(key);
            IEqualityComparer<TValue> equalityComparer = (IEqualityComparer<TValue>)EqualityComparer<TValue>.Default;
            label_3:
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_buckets;
            int bucketNo;
            int lockNo;
            this.GetBucketAndLockNo(hashCode, out bucketNo, out lockNo, buckets.Length);
            lock (this.m_locks[lockNo])
            {
                if (buckets == this.m_buckets)
                {
                    ConcurrentDictionary<TKey, TValue>.Node node1 = (ConcurrentDictionary<TKey, TValue>.Node)null;
                    for (ConcurrentDictionary<TKey, TValue>.Node next = buckets[bucketNo]; next != null; next = next.m_next)
                    {
                        if (this.m_comparer.Equals(next.m_key, key))
                        {
                            if (!equalityComparer.Equals(next.m_value, comparisonValue))
                                return false;
                            ConcurrentDictionary<TKey, TValue>.Node node2 = new ConcurrentDictionary<TKey, TValue>.Node(next.m_key, newValue, hashCode, next.m_next);
                            if (node1 == null)
                                buckets[bucketNo] = node2;
                            else
                                node1.m_next = node2;
                            return true;
                        }
                        node1 = next;
                    }
                    return false;
                }
                goto label_3;
            }
        }

        public void Clear()
        {
            int locksAcquired = 0;
            try
            {
                this.AcquireAllLocks(ref locksAcquired);
                this.m_buckets = new ConcurrentDictionary<TKey, TValue>.Node[31];
                Array.Clear((Array)this.m_countPerLock, 0, this.m_countPerLock.Length);
            }
            finally
            {
                this.ReleaseLocks(0, locksAcquired);
            }
        }

        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            int locksAcquired = 0;
            try
            {
                this.AcquireAllLocks(ref locksAcquired);
                int length = 0;
                int index = 0;
                while (index < this.m_locks.Length)
                {
                    checked { length += this.m_countPerLock[index]; }
                    checked { ++index; }
                }
                KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[length];
                this.CopyToPairs(array, 0);
                return array;
            }
            finally
            {
                this.ReleaseLocks(0, locksAcquired);
            }
        }

        private void CopyToPairs(KeyValuePair<TKey, TValue>[] array, int index)
        {
            foreach (ConcurrentDictionary<TKey, TValue>.Node bucket in this.m_buckets)
            {
                for (ConcurrentDictionary<TKey, TValue>.Node node = bucket; node != null; node = node.m_next)
                {
                    array[index] = new KeyValuePair<TKey, TValue>(node.m_key, node.m_value);
                    ++index;
                }
            }
        }

        private void CopyToEntries(DictionaryEntry[] array, int index)
        {
            foreach (ConcurrentDictionary<TKey, TValue>.Node bucket in this.m_buckets)
            {
                for (ConcurrentDictionary<TKey, TValue>.Node node = bucket; node != null; node = node.m_next)
                {
                    array[index] = new DictionaryEntry((object)node.m_key, (object)node.m_value);
                    ++index;
                }
            }
        }

        private void CopyToObjects(object[] array, int index)
        {
            foreach (ConcurrentDictionary<TKey, TValue>.Node bucket in this.m_buckets)
            {
                for (ConcurrentDictionary<TKey, TValue>.Node node = bucket; node != null; node = node.m_next)
                {
                    array[index] = (object)new KeyValuePair<TKey, TValue>(node.m_key, node.m_value);
                    ++index;
                }
            }
        }
        
        private bool TryAddInternal(TKey key, TValue value, bool updateIfExists, bool acquireLock, out TValue resultingValue)
        {
            int hashCode = this.m_comparer.GetHashCode(key);
            label_1:
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_buckets;
            int bucketNo;
            int lockNo;
            this.GetBucketAndLockNo(hashCode, out bucketNo, out lockNo, buckets.Length);
            bool flag = false;
            bool taken = false;
            try
            {
                if (acquireLock)
                    Monitor2.Enter(this.m_locks[lockNo], ref taken);
                if (buckets == this.m_buckets)
                {
                    ConcurrentDictionary<TKey, TValue>.Node node1 = (ConcurrentDictionary<TKey, TValue>.Node)null;
                    for (ConcurrentDictionary<TKey, TValue>.Node next = buckets[bucketNo]; next != null; next = next.m_next)
                    {
                        if (this.m_comparer.Equals(next.m_key, key))
                        {
                            if (updateIfExists)
                            {
                                ConcurrentDictionary<TKey, TValue>.Node node2 = new ConcurrentDictionary<TKey, TValue>.Node(next.m_key, value, hashCode, next.m_next);
                                if (node1 == null)
                                    buckets[bucketNo] = node2;
                                else
                                    node1.m_next = node2;
                                resultingValue = value;
                            }
                            else
                                resultingValue = next.m_value;
                            return false;
                        }
                        node1 = next;
                    }
                    buckets[bucketNo] = new ConcurrentDictionary<TKey, TValue>.Node(key, value, hashCode, buckets[bucketNo]);
                    checked { ++this.m_countPerLock[lockNo]; }
                    if (this.m_countPerLock[lockNo] > buckets.Length / this.m_locks.Length)
                        flag = true;
                }
                else
                    goto label_1;
            }
            finally
            {
                if (taken)
                    Monitor.Exit(this.m_locks[lockNo]);
            }
            if (flag)
                this.GrowTable(buckets);
            resultingValue = value;
            return true;
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue obj;
                if (!this.TryGetValue(key, out obj))
                    throw new KeyNotFoundException();
                return obj;
            }
            set
            {
                if ((object)key == null)
                    throw new ArgumentNullException(nameof(key));
                TValue resultingValue;
                this.TryAddInternal(key, value, true, true, out resultingValue);
            }
        }

        public int Count
        {
            get
            {
                int num = 0;
                int locksAcquired = 0;
                try
                {
                    this.AcquireAllLocks(ref locksAcquired);
                    for (int index = 0; index < this.m_countPerLock.Length; ++index)
                        num += this.m_countPerLock[index];
                }
                finally
                {
                    this.ReleaseLocks(0, locksAcquired);
                }
                return num;
            }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if ((object)key == null)
                throw new ArgumentNullException(nameof(key));
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));
            TValue resultingValue;
            if (this.TryGetValue(key, out resultingValue))
                return resultingValue;
            this.TryAddInternal(key, valueFactory(key), false, true, out resultingValue);
            return resultingValue;
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            if ((object)key == null)
                throw new ArgumentNullException(nameof(key));
            TValue resultingValue;
            this.TryAddInternal(key, value, false, true, out resultingValue);
            return resultingValue;
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if ((object)key == null)
                throw new ArgumentNullException(nameof(key));
            if (addValueFactory == null)
                throw new ArgumentNullException(nameof(addValueFactory));
            if (updateValueFactory == null)
                throw new ArgumentNullException(nameof(updateValueFactory));
            TValue comparisonValue;
            TValue newValue;
            do
            {
                while (!this.TryGetValue(key, out comparisonValue))
                {
                    TValue obj = addValueFactory(key);
                    TValue resultingValue;
                    if (this.TryAddInternal(key, obj, false, true, out resultingValue))
                        return resultingValue;
                }
                newValue = updateValueFactory(key, comparisonValue);
            }
            while (!this.TryUpdate(key, newValue, comparisonValue));
            return newValue;
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if ((object)key == null)
                throw new ArgumentNullException(nameof(key));
            if (updateValueFactory == null)
                throw new ArgumentNullException(nameof(updateValueFactory));
            TValue comparisonValue;
            TValue newValue;
            do
            {
                while (!this.TryGetValue(key, out comparisonValue))
                {
                    TValue resultingValue;
                    if (this.TryAddInternal(key, addValue, false, true, out resultingValue))
                        return resultingValue;
                }
                newValue = updateValueFactory(key, comparisonValue);
            }
            while (!this.TryUpdate(key, newValue, comparisonValue));
            return newValue;
        }

        public bool IsEmpty
        {
            get
            {
                int locksAcquired = 0;
                try
                {
                    this.AcquireAllLocks(ref locksAcquired);
                    for (int index = 0; index < this.m_countPerLock.Length; ++index)
                    {
                        if (this.m_countPerLock[index] != 0)
                            return false;
                    }
                }
                finally
                {
                    this.ReleaseLocks(0, locksAcquired);
                }
                return true;
            }
        }


        public ICollection<TKey> Keys
        {
            get
            {
                return (ICollection<TKey>)this.GetKeys();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return (ICollection<TValue>)this.GetValues();
            }
        }
        private void GrowTable(ConcurrentDictionary<TKey, TValue>.Node[] buckets)
        {
            int locksAcquired = 0;
            try
            {
                this.AcquireLocks(0, 1, ref locksAcquired);
                if (buckets != this.m_buckets)
                    return;
                int length;
                try
                {
                    length = checked(buckets.Length * 2 + 1);
                    while (true)
                    {
                        if (length % 3 != 0 && length % 5 != 0)
                            goto label_5;
                        label_3:
                        checked { length += 2; }
                        continue;
                        label_5:
                        if (length % 7 == 0)
                            goto label_3;
                        else
                            break;
                    }
                }
                catch (OverflowException)
                {
                    return;
                }
                ConcurrentDictionary<TKey, TValue>.Node[] nodeArray = new ConcurrentDictionary<TKey, TValue>.Node[length];
                int[] numArray = new int[this.m_locks.Length];
                this.AcquireLocks(1, this.m_locks.Length, ref locksAcquired);
                ConcurrentDictionary<TKey, TValue>.Node next;
                for (int index = 0; index < buckets.Length; ++index)
                {
                    for (ConcurrentDictionary<TKey, TValue>.Node node = buckets[index]; node != null; node = next)
                    {
                        next = node.m_next;
                        int bucketNo;
                        int lockNo;
                        this.GetBucketAndLockNo(node.m_hashcode, out bucketNo, out lockNo, nodeArray.Length);
                        nodeArray[bucketNo] = new ConcurrentDictionary<TKey, TValue>.Node(node.m_key, node.m_value, node.m_hashcode, nodeArray[bucketNo]);
                        checked { ++numArray[lockNo]; }
                    }
                }
                this.m_buckets = nodeArray;
                this.m_countPerLock = numArray;
            }
            finally
            {
                this.ReleaseLocks(0, locksAcquired);
            }
        }

        private void GetBucketAndLockNo(int hashcode, out int bucketNo, out int lockNo, int bucketCount)
        {
            bucketNo = (hashcode & int.MaxValue) % bucketCount;
            lockNo = bucketNo % this.m_locks.Length;
        }

        private static int DefaultConcurrencyLevel
        {
            get
            {
                return 4 * Environment.ProcessorCount;
            }
        }

        private void AcquireAllLocks(ref int locksAcquired)
        {
            this.AcquireLocks(0, this.m_locks.Length, ref locksAcquired);
        }

        private void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired)
        {
            for (int index = fromInclusive; index < toExclusive; ++index)
            {
                bool taken = false;
                try
                {
                    Monitor2.Enter(this.m_locks[index], ref taken);
                }
                finally
                {
                    if (taken)
                        ++locksAcquired;
                }
            }
        }

        private void ReleaseLocks(int fromInclusive, int toExclusive)
        {
            for (int index = fromInclusive; index < toExclusive; ++index)
                Monitor.Exit(this.m_locks[index]);
        }

        private ReadOnlyCollection<TKey> GetKeys()
        {
            int locksAcquired = 0;
            try
            {
                this.AcquireAllLocks(ref locksAcquired);
                List<TKey> keyList = new List<TKey>();
                for (int index = 0; index < this.m_buckets.Length; ++index)
                {
                    for (ConcurrentDictionary<TKey, TValue>.Node node = this.m_buckets[index]; node != null; node = node.m_next)
                        keyList.Add(node.m_key);
                }
                return new ReadOnlyCollection<TKey>((IList<TKey>)keyList);
            }
            finally
            {
                this.ReleaseLocks(0, locksAcquired);
            }
        }

        private ReadOnlyCollection<TValue> GetValues()
        {
            int locksAcquired = 0;
            try
            {
                this.AcquireAllLocks(ref locksAcquired);
                List<TValue> objList = new List<TValue>();
                for (int index = 0; index < this.m_buckets.Length; ++index)
                {
                    for (ConcurrentDictionary<TKey, TValue>.Node node = this.m_buckets[index]; node != null; node = node.m_next)
                        objList.Add(node.m_value);
                }
                return new ReadOnlyCollection<TValue>((IList<TValue>)objList);
            }
            finally
            {
                this.ReleaseLocks(0, locksAcquired);
            }
        }

        [Conditional("DEBUG")]
        private void Assert(bool condition)
        {
            Debug.Assert(condition);
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            this.m_serializationArray = this.ToArray();
            this.m_serializationConcurrencyLevel = this.m_locks.Length;
            this.m_serializationCapacity = this.m_buckets.Length;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            KeyValuePair<TKey, TValue>[] serializationArray = this.m_serializationArray;
            this.m_buckets = new ConcurrentDictionary<TKey, TValue>.Node[this.m_serializationCapacity];
            this.m_countPerLock = new int[this.m_serializationConcurrencyLevel];
            this.m_locks = new object[this.m_serializationConcurrencyLevel];
            for (int index = 0; index < this.m_locks.Length; ++index)
                this.m_locks[index] = new object();
            this.InitializeFromCollection((IEnumerable<KeyValuePair<TKey, TValue>>)serializationArray);
            this.m_serializationArray = (KeyValuePair<TKey, TValue>[])null;
        }

        private class Node
        {
            internal TKey m_key;
            internal TValue m_value;
            internal volatile ConcurrentDictionary<TKey, TValue>.Node m_next;
            internal int m_hashcode;

            internal Node(TKey key, TValue value, int hashcode)
              : this(key, value, hashcode, (ConcurrentDictionary<TKey, TValue>.Node)null)
            {
            }

            internal Node(TKey key, TValue value, int hashcode, ConcurrentDictionary<TKey, TValue>.Node next)
            {
                this.m_key = key;
                this.m_value = value;
                this.m_next = next;
                this.m_hashcode = hashcode;
            }
        }
    }

    internal class Monitor2
    {
        internal static void Enter(object obj, ref bool taken)
        {
            Monitor.Enter(obj);
            taken = true;
        }

        internal static bool TryEnter(object obj)
        {
            return Monitor.TryEnter(obj);
        }

        internal static void TryEnter(object obj, ref bool taken)
        {
            taken = Monitor.TryEnter(obj);
        }

        internal static bool TryEnter(object obj, int millisecondsTimeout)
        {
            return Monitor.TryEnter(obj, millisecondsTimeout);
        }

        internal static bool TryEnter(object obj, TimeSpan timeout)
        {
            return Monitor.TryEnter(obj, timeout);
        }

        internal static void TryEnter(object obj, int millisecondsTimeout, ref bool taken)
        {
            taken = Monitor.TryEnter(obj, millisecondsTimeout);
        }
    }
}

#endif
