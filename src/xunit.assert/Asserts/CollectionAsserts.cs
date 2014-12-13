using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace Xunit
{
    public partial class Assert
    {
        /// <summary>
        /// Verifies that all items in the collection pass when executed against
        /// action.
        /// </summary>
        /// <typeparam name="T">The type of the object to be verified</typeparam>
        /// <param name="collection">The collection</param>
        /// <param name="action">The action to test each item against</param>
        /// <exception cref="AllException">Thrown when the collection contains at least one non-matching element</exception>
        public static void All<T>(IEnumerable<T> collection, Action<T> action)
        {
            Assert.GuardArgumentNotNull("collection", collection);
            Assert.GuardArgumentNotNull("action", action);

            var errors = new Stack<Tuple<int, Exception>>();
            var array = collection.ToArray();

            for (var idx = 0; idx < array.Length; ++idx)
            {
                try
                {
                    action(array[idx]);
                }
                catch (Exception ex)
                {
                    errors.Push(new Tuple<int, Exception>(idx, ex));
                }
            }

            if (errors.Count > 0)
                throw new AllException(array.Length, errors.ToArray());
        }

        /// <summary>
        /// Verifies that a collection contains exactly a given number of elements, which meet
        /// the criteria provided by the element inspectors.
        /// </summary>
        /// <typeparam name="T">The type of the object to be verified</typeparam>
        /// <param name="collection">The collection to be inspected</param>
        /// <param name="elementInspectors">The element inspectors, which inspect each element in turn. The
        /// total number of element inspectors must exactly match the number of elements in the collection.</param>
        public static void Collection<T>(IEnumerable<T> collection, params Action<T>[] elementInspectors)
        {
            T[] elements = collection.ToArray();
            int expectedCount = elementInspectors.Length;
            int actualCount = elements.Length;

            if (expectedCount != actualCount)
                throw new CollectionException(expectedCount, actualCount);

            for (int idx = 0; idx < actualCount; idx++)
            {
                try
                {
                    elementInspectors[idx](elements[idx]);
                }
                catch (Exception ex)
                {
                    throw new CollectionException(expectedCount, actualCount, idx, ex);
                }
            }
        }

        /// <summary>
        /// Verifies that a collection contains a given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be verified</typeparam>
        /// <param name="expected">The object expected to be in the collection</param>
        /// <param name="collection">The collection to be inspected</param>
        /// <exception cref="ContainsException">Thrown when the object is not present in the collection</exception>
        public static void Contains<T>(T expected, IEnumerable<T> collection)
        {
            Contains(expected, collection, GetEqualityComparer<T>());
        }

        /// <summary>
        /// Verifies that a collection contains a given object, using an equality comparer.
        /// </summary>
        /// <typeparam name="T">The type of the object to be verified</typeparam>
        /// <param name="expected">The object expected to be in the collection</param>
        /// <param name="collection">The collection to be inspected</param>
        /// <param name="comparer">The comparer used to equate objects in the collection with the expected object</param>
        /// <exception cref="ContainsException">Thrown when the object is not present in the collection</exception>
        public static void Contains<T>(T expected, IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            Assert.GuardArgumentNotNull("comparer", comparer);
            Assert.GuardArgumentNotNull("collection", collection);

            foreach (var item in collection)
                if (comparer.Equals(expected, item))
                    return;

            throw new ContainsException(expected, collection);
        }

        /// <summary>
        /// Verifies that a collection contains a given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be verified</typeparam>
        /// <param name="collection">The collection to be inspected</param>
        /// <param name="filter">The filter used to find the item you're ensuring the collection contains</param>
        /// <exception cref="ContainsException">Thrown when the object is not present in the collection</exception>
        public static void Contains<T>(IEnumerable<T> collection, Predicate<T> filter)
        {
            Assert.GuardArgumentNotNull("collection", collection);
            Assert.GuardArgumentNotNull("filter", filter);

            foreach (var item in collection)
                if (filter(item))
                    return;

            throw new ContainsException("(filter expression)", collection);
        }

        /// <summary>
        /// Verifies that a collection does not contain a given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be compared</typeparam>
        /// <param name="expected">The object that is expected not to be in the collection</param>
        /// <param name="collection">The collection to be inspected</param>
        /// <exception cref="DoesNotContainException">Thrown when the object is present inside the container</exception>
        public static void DoesNotContain<T>(T expected, IEnumerable<T> collection)
        {
            DoesNotContain(expected, collection, GetEqualityComparer<T>());
        }

        /// <summary>
        /// Verifies that a collection does not contain a given object, using an equality comparer.
        /// </summary>
        /// <typeparam name="T">The type of the object to be compared</typeparam>
        /// <param name="expected">The object that is expected not to be in the collection</param>
        /// <param name="collection">The collection to be inspected</param>
        /// <param name="comparer">The comparer used to equate objects in the collection with the expected object</param>
        /// <exception cref="DoesNotContainException">Thrown when the object is present inside the container</exception>
        public static void DoesNotContain<T>(T expected, IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            Assert.GuardArgumentNotNull("collection", collection);
            Assert.GuardArgumentNotNull("comparer", comparer);

            foreach (var item in collection)
                if (comparer.Equals(expected, item))
                    throw new DoesNotContainException(expected, collection);
        }

        /// <summary>
        /// Verifies that a collection does not contain a given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be compared</typeparam>
        /// <param name="collection">The collection to be inspected</param>
        /// <param name="filter">The filter used to find the item you're ensuring the collection does not contain</param>
        /// <exception cref="DoesNotContainException">Thrown when the object is present inside the container</exception>
        public static void DoesNotContain<T>(IEnumerable<T> collection, Predicate<T> filter)
        {
            Assert.GuardArgumentNotNull("collection", collection);
            Assert.GuardArgumentNotNull("filter", filter);

            foreach (var item in collection)
                if (filter(item))
                    throw new DoesNotContainException("(filter expression)", collection);
        }

        /// <summary>
        /// Verifies that a collection is empty.
        /// </summary>
        /// <param name="collection">The collection to be inspected</param>
        /// <exception cref="ArgumentNullException">Thrown when the collection is null</exception>
        /// <exception cref="EmptyException">Thrown when the collection is not empty</exception>
        public static void Empty(IEnumerable collection)
        {
            Assert.GuardArgumentNotNull("collection", collection);

            if (collection.GetEnumerator().MoveNext())
                throw new EmptyException();
        }

        /// <summary>
        /// Verifies that two sequences are equivalent, using a default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The value to be compared against</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal</exception>
        public static void Equal<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            Equal<IEnumerable<T>>(expected, actual, GetEqualityComparer<IEnumerable<T>>(true));
        }

        /// <summary>
        /// Verifies that two sequences are equivalent, using a custom equatable comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="comparer">The comparer used to compare the two objects</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal</exception>
        public static void Equal<T>(IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T> comparer)
        {
            Equal<IEnumerable<T>>(expected, actual, GetEqualityComparer<IEnumerable<T>>(true, new AssertEqualityComparerAdapter<T>(comparer)));
        }

        /// <summary>
        /// Verifies that a collection is not empty.
        /// </summary>
        /// <param name="collection">The collection to be inspected</param>
        /// <exception cref="ArgumentNullException">Thrown when a null collection is passed</exception>
        /// <exception cref="NotEmptyException">Thrown when the collection is empty</exception>
        public static void NotEmpty(IEnumerable collection)
        {
            Assert.GuardArgumentNotNull("collection", collection);

            if (!collection.GetEnumerator().MoveNext())
                throw new NotEmptyException();
        }

        /// <summary>
        /// Verifies that two sequences are not equivalent, using a default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The actual object</param>
        /// <exception cref="NotEqualException">Thrown when the objects are equal</exception>
        public static void NotEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            NotEqual<IEnumerable<T>>(expected, actual, GetEqualityComparer<IEnumerable<T>>(true));
        }

        /// <summary>
        /// Verifies that two sequences are not equivalent, using a custom equality comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The actual object</param>
        /// <param name="comparer">The comparer used to compare the two objects</param>
        /// <exception cref="NotEqualException">Thrown when the objects are equal</exception>
        public static void NotEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T> comparer)
        {
            NotEqual<IEnumerable<T>>(expected, actual, GetEqualityComparer<IEnumerable<T>>(true, new AssertEqualityComparerAdapter<T>(comparer)));
        }

        /// <summary>
        /// Verifies that the given collection contains only a single
        /// element of the given type.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns>The single item in the collection.</returns>
        /// <exception cref="SingleException">Thrown when the collection does not contain
        /// exactly one element.</exception>
        public static object Single(IEnumerable collection)
        {
            return Single(collection.Cast<object>());
        }

        /// <summary>
        /// Verifies that the given collection contains only a single
        /// element of the given value. The collection may or may not
        /// contain other values.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="expected">The value to find in the collection.</param>
        /// <returns>The single item in the collection.</returns>
        /// <exception cref="SingleException">Thrown when the collection does not contain
        /// exactly one element.</exception>
        public static void Single(IEnumerable collection, object expected)
        {
            Single(collection.Cast<object>(), item => Object.Equals(item, expected));
        }

        /// <summary>
        /// Verifies that the given collection contains only a single
        /// element of the given type.
        /// </summary>
        /// <typeparam name="T">The collection type.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns>The single item in the collection.</returns>
        /// <exception cref="SingleException">Thrown when the collection does not contain
        /// exactly one element.</exception>
        public static T Single<T>(IEnumerable<T> collection)
        {
            return Single(collection, item => true);
        }

        /// <summary>
        /// Verifies that the given collection contains only a single
        /// element of the given type which matches the given predicate. The
        /// collection may or may not contain other values which do not
        /// match the given predicate.
        /// </summary>
        /// <typeparam name="T">The collection type.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="predicate">The item matching predicate.</param>
        /// <returns>The single item in the filtered collection.</returns>
        /// <exception cref="SingleException">Thrown when the filtered collection does
        /// not contain exactly one element.</exception>
        public static T Single<T>(IEnumerable<T> collection, Predicate<T> predicate)
        {
            Assert.GuardArgumentNotNull("collection", collection);
            Assert.GuardArgumentNotNull("predicate", predicate);

            int count = 0;
            T result = default(T);

            foreach (T item in collection)
                if (predicate(item))
                {
                    result = item;
                    ++count;
                }

            if (count != 1)
                throw new SingleException(count);

            return result;
        }
    }
}