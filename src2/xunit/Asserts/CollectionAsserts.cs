using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit.Sdk;

namespace Xunit
{
    public partial class Assert
    {
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

            // REVIEW: Should this print out the contents of the collection when it fails?
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
            Guard.ArgumentNotNull("comparer", comparer);

            if (collection != null)
                foreach (T item in collection)
                    if (comparer.Equals(expected, item))
                        return;

            throw new ContainsException(expected);
        }

        /// <summary>
        /// Verifies that a string contains a given sub-string, using the current culture.
        /// </summary>
        /// <param name="expectedSubstring">The sub-string expected to be in the string</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <exception cref="ContainsException">Thrown when the sub-string is not present inside the string</exception>
        public static void Contains(string expectedSubstring, string actualString)
        {
            Contains(expectedSubstring, actualString, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Verifies that a string contains a given sub-string, using the given comparison type.
        /// </summary>
        /// <param name="expectedSubstring">The sub-string expected to be in the string</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <param name="comparisonType">The type of string comparison to perform</param>
        /// <exception cref="ContainsException">Thrown when the sub-string is not present inside the string</exception>
        public static void Contains(string expectedSubstring, string actualString, StringComparison comparisonType)
        {
            if (actualString == null || actualString.IndexOf(expectedSubstring, comparisonType) < 0)
                throw new ContainsException(expectedSubstring, actualString);
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
            Guard.ArgumentNotNull("comparer", comparer);

            if (collection != null)
                foreach (T item in collection)
                    if (comparer.Equals(expected, item))
                        throw new DoesNotContainException(expected);
        }

        /// <summary>
        /// Verifies that a string does not contain a given sub-string, using the current culture.
        /// </summary>
        /// <param name="expectedSubstring">The sub-string which is expected not to be in the string</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <exception cref="DoesNotContainException">Thrown when the sub-string is present inside the string</exception>
        public static void DoesNotContain(string expectedSubstring, string actualString)
        {
            DoesNotContain(expectedSubstring, actualString, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Verifies that a string does not contain a given sub-string, using the current culture.
        /// </summary>
        /// <param name="expectedSubstring">The sub-string which is expected not to be in the string</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <param name="comparisonType">The type of string comparison to perform</param>
        /// <exception cref="DoesNotContainException">Thrown when the sub-string is present inside the given string</exception>
        public static void DoesNotContain(string expectedSubstring, string actualString, StringComparison comparisonType)
        {
            if (actualString != null && actualString.IndexOf(expectedSubstring, comparisonType) >= 0)
                throw new DoesNotContainException(expectedSubstring);
        }

        /// <summary>
        /// Verifies that a collection is empty.
        /// </summary>
        /// <param name="collection">The collection to be inspected</param>
        /// <exception cref="ArgumentNullException">Thrown when the collection is null</exception>
        /// <exception cref="EmptyException">Thrown when the collection is not empty</exception>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "object", Justification = "No can do")]
        public static void Empty(IEnumerable collection)
        {
            Guard.ArgumentNotNull("collection", collection);

#pragma warning disable 168
            foreach (object @object in collection)
                throw new EmptyException();
#pragma warning restore 168
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
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "object", Justification = "No can do")]
        public static void NotEmpty(IEnumerable collection)
        {
            Guard.ArgumentNotNull("collection", collection);

#pragma warning disable 168
            foreach (object @object in collection)
                return;
#pragma warning restore 168

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
            Guard.ArgumentNotNull("collection", collection);

            int count = 0;
            object result = null;

            foreach (object item in collection)
            {
                result = item;
                ++count;
            }

            if (count != 1)
                throw new SingleException(count);

            return result;
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
            Guard.ArgumentNotNull("collection", collection);

            int count = 0;

            foreach (object item in collection)
                if (Object.Equals(item, expected))
                    ++count;

            if (count != 1)
                throw new SingleException(count, expected);
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
            Guard.ArgumentNotNull("collection", collection);
            Guard.ArgumentNotNull("predicate", predicate);

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