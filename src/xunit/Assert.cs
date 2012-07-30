using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Contains various static methods that are used to verify that conditions are met during the
    /// process of running tests.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "This is not marked as static because we want people to be able to derive from it")]
    public class Assert
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Assert"/> class.
        /// </summary>
        protected Assert() { }

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
        /// Verifies that a block of code does not throw any exceptions.
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        public static void DoesNotThrow(Action testCode)
        {
            Exception ex = Record.Exception(testCode);

            if (ex != null)
                throw new DoesNotThrowException(ex);
        }

        /// <summary>
        /// Verifies that a block of code does not throw any exceptions.
        /// </summary>
        /// <param name="testCode">A delegate to the code to be tested</param>
        public static void DoesNotThrow(Func<object> testCode)
        {
            Exception ex = Record.Exception(testCode);

            if (ex != null)
                throw new DoesNotThrowException(ex);
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
        /// Verifies that two objects are equal, using a default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The value to be compared against</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal</exception>
        public static void Equal<T>(T expected, T actual)
        {
            Equal(expected, actual, GetEqualityComparer<T>());
        }

        /// <summary>
        /// Verifies that two objects are equal, using a custom equatable comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="comparer">The comparer used to compare the two objects</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal</exception>
        public static void Equal<T>(T expected, T actual, IEqualityComparer<T> comparer)
        {
            Guard.ArgumentNotNull("comparer", comparer);

            if (!comparer.Equals(expected, actual))
                throw new EqualException(expected, actual);
        }

        /// <summary>
        /// Verifies that two <see cref="double"/> values are equal, within the number of decimal
        /// places given by <paramref name="precision"/>.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="precision">The number of decimal places (valid values: 0-15)</param>
        /// <exception cref="EqualException">Thrown when the values are not equal</exception>
        public static void Equal(double expected, double actual, int precision)
        {
            var expectedRounded = Math.Round(expected, precision);
            var actualRounded = Math.Round(actual, precision);

            if (!GetEqualityComparer<double>().Equals(expectedRounded, actualRounded))
                throw new EqualException(
                    String.Format(CultureInfo.CurrentCulture, "{0} (rounded from {1})", expectedRounded, expected),
                    String.Format(CultureInfo.CurrentCulture, "{0} (rounded from {1})", actualRounded, actual),
                    skipPositionCheck: true
                );
        }

        /// <summary>
        /// Verifies that two <see cref="decimal"/> values are equal, within the number of decimal
        /// places given by <paramref name="precision"/>.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="precision">The number of decimal places (valid values: 0-15)</param>
        /// <exception cref="EqualException">Thrown when the values are not equal</exception>
        public static void Equal(decimal expected, decimal actual, int precision)
        {
            var expectedRounded = Math.Round(expected, precision);
            var actualRounded = Math.Round(actual, precision);

            if (!GetEqualityComparer<decimal>().Equals(expectedRounded, actualRounded))
                throw new EqualException(
                    String.Format(CultureInfo.CurrentCulture, "{0} (rounded from {1})", expectedRounded, expected),
                    String.Format(CultureInfo.CurrentCulture, "{0} (rounded from {1})", actualRounded, actual),
                    skipPositionCheck: true
                );
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

        /// <summary>Do not call this method.</summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "a", Justification = "We do not control the signature of this method.")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "b", Justification = "We do not control the signature of this method.")]
        [Obsolete("This is an override of Object.Equals(). Call Assert.Equal() instead.", true)]
        public new static bool Equals(object a, object b)
        {
            throw new InvalidOperationException("Assert.Equals should not be used");
        }

        /// <summary>
        /// Verifies that the condition is false.
        /// </summary>
        /// <param name="condition">The condition to be tested</param>
        /// <exception cref="FalseException">Thrown if the condition is not false</exception>
        public static void False(bool condition)
        {
            False(condition, null);
        }

        /// <summary>
        /// Verifies that the condition is false.
        /// </summary>
        /// <param name="condition">The condition to be tested</param>
        /// <param name="userMessage">The message to show when the condition is not false</param>
        /// <exception cref="FalseException">Thrown if the condition is not false</exception>
        public static void False(bool condition, string userMessage)
        {
            if (condition)
                throw new FalseException(userMessage);
        }

        static IComparer<T> GetComparer<T>() where T : IComparable
        {
            return new AssertComparer<T>();
        }

        static IEqualityComparer<T> GetEqualityComparer<T>(bool skipTypeCheck = false, IEqualityComparer innerComparer = null)
        {
            return new AssertEqualityComparer<T>(skipTypeCheck, innerComparer);
        }

        /// <summary>
        /// Verifies that a value is within a given range.
        /// </summary>
        /// <typeparam name="T">The type of the value to be compared</typeparam>
        /// <param name="actual">The actual value to be evaluated</param>
        /// <param name="low">The (inclusive) low value of the range</param>
        /// <param name="high">The (inclusive) high value of the range</param>
        /// <exception cref="InRangeException">Thrown when the value is not in the given range</exception>
        public static void InRange<T>(T actual, T low, T high) where T : IComparable
        {
            InRange(actual, low, high, GetComparer<T>());
        }

        /// <summary>
        /// Verifies that a value is within a given range, using a comparer.
        /// </summary>
        /// <typeparam name="T">The type of the value to be compared</typeparam>
        /// <param name="actual">The actual value to be evaluated</param>
        /// <param name="low">The (inclusive) low value of the range</param>
        /// <param name="high">The (inclusive) high value of the range</param>
        /// <param name="comparer">The comparer used to evaluate the value's range</param>
        /// <exception cref="InRangeException">Thrown when the value is not in the given range</exception>
        public static void InRange<T>(T actual, T low, T high, IComparer<T> comparer)
        {
            Guard.ArgumentNotNull("comparer", comparer);

            if (comparer.Compare(low, actual) > 0 || comparer.Compare(actual, high) > 0)
                throw new InRangeException(actual, low, high);
        }

        /// <summary>
        /// Verifies that an object is of the given type or a derived type.
        /// </summary>
        /// <typeparam name="T">The type the object should be</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <returns>The object, casted to type T when successful</returns>
        /// <exception cref="IsAssignableFromException">Thrown when the object is not the given type</exception>
        public static T IsAssignableFrom<T>(object @object)
        {
            IsAssignableFrom(typeof(T), @object);
            return (T)@object;
        }

        /// <summary>
        /// Verifies that an object is of the given type or a derived type.
        /// </summary>
        /// <param name="expectedType">The type the object should be</param>
        /// <param name="object">The object to be evaluated</param>
        /// <exception cref="IsAssignableFromException">Thrown when the object is not the given type</exception>
        public static void IsAssignableFrom(Type expectedType, object @object)
        {
            Guard.ArgumentNotNull("expectedType", expectedType);

            if (@object == null || !expectedType.IsAssignableFrom(@object.GetType()))
                throw new IsAssignableFromException(expectedType, @object);
        }

        /// <summary>
        /// Verifies that an object is not exactly the given type.
        /// </summary>
        /// <typeparam name="T">The type the object should not be</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <exception cref="IsNotTypeException">Thrown when the object is the given type</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The generic version is a more convenient shorthand than typeof")]
        public static void IsNotType<T>(object @object)
        {
            IsNotType(typeof(T), @object);
        }

        /// <summary>
        /// Verifies that an object is not exactly the given type.
        /// </summary>
        /// <param name="expectedType">The type the object should not be</param>
        /// <param name="object">The object to be evaluated</param>
        /// <exception cref="IsNotTypeException">Thrown when the object is the given type</exception>
        public static void IsNotType(Type expectedType, object @object)
        {
            Guard.ArgumentNotNull("expectedType", expectedType);

            if (@object != null && expectedType.Equals(@object.GetType()))
                throw new IsNotTypeException(expectedType, @object);
        }

        /// <summary>
        /// Verifies that an object is exactly the given type (and not a derived type).
        /// </summary>
        /// <typeparam name="T">The type the object should be</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <returns>The object, casted to type T when successful</returns>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public static T IsType<T>(object @object)
        {
            IsType(typeof(T), @object);
            return (T)@object;
        }

        /// <summary>
        /// Verifies that an object is exactly the given type (and not a derived type).
        /// </summary>
        /// <param name="expectedType">The type the object should be</param>
        /// <param name="object">The object to be evaluated</param>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public static void IsType(Type expectedType, object @object)
        {
            Guard.ArgumentNotNull("expectedType", expectedType);

            if (@object == null || !expectedType.Equals(@object.GetType()))
                throw new IsTypeException(expectedType, @object);
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
        /// Verifies that two objects are not equal, using a default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The actual object</param>
        /// <exception cref="NotEqualException">Thrown when the objects are equal</exception>
        public static void NotEqual<T>(T expected, T actual)
        {
            NotEqual(expected, actual, GetEqualityComparer<T>());
        }

        /// <summary>
        /// Verifies that two objects are not equal, using a custom equality comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The actual object</param>
        /// <param name="comparer">The comparer used to examine the objects</param>
        /// <exception cref="NotEqualException">Thrown when the objects are equal</exception>
        public static void NotEqual<T>(T expected, T actual, IEqualityComparer<T> comparer)
        {
            Guard.ArgumentNotNull("comparer", comparer);

            if (comparer.Equals(expected, actual))
                throw new NotEqualException();
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
        /// Verifies that a value is not within a given range, using the default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the value to be compared</typeparam>
        /// <param name="actual">The actual value to be evaluated</param>
        /// <param name="low">The (inclusive) low value of the range</param>
        /// <param name="high">The (inclusive) high value of the range</param>
        /// <exception cref="NotInRangeException">Thrown when the value is in the given range</exception>
        public static void NotInRange<T>(T actual, T low, T high) where T : IComparable
        {
            NotInRange(actual, low, high, GetComparer<T>());
        }

        /// <summary>
        /// Verifies that a value is not within a given range, using a comparer.
        /// </summary>
        /// <typeparam name="T">The type of the value to be compared</typeparam>
        /// <param name="actual">The actual value to be evaluated</param>
        /// <param name="low">The (inclusive) low value of the range</param>
        /// <param name="high">The (inclusive) high value of the range</param>
        /// <param name="comparer">The comparer used to evaluate the value's range</param>
        /// <exception cref="NotInRangeException">Thrown when the value is in the given range</exception>
        public static void NotInRange<T>(T actual, T low, T high, IComparer<T> comparer)
        {
            Guard.ArgumentNotNull("comparer", comparer);

            if (comparer.Compare(low, actual) <= 0 && comparer.Compare(actual, high) <= 0)
                throw new NotInRangeException(actual, low, high);
        }

        /// <summary>
        /// Verifies that an object reference is not null.
        /// </summary>
        /// <param name="object">The object to be validated</param>
        /// <exception cref="NotNullException">Thrown when the object is not null</exception>
        public static void NotNull(object @object)
        {
            if (@object == null)
                throw new NotNullException();
        }

        /// <summary>
        /// Verifies that two objects are not the same instance.
        /// </summary>
        /// <param name="expected">The expected object instance</param>
        /// <param name="actual">The actual object instance</param>
        /// <exception cref="NotSameException">Thrown when the objects are the same instance</exception>
        public static void NotSame(object expected, object actual)
        {
            if (object.ReferenceEquals(expected, actual))
                throw new NotSameException();
        }

        /// <summary>
        /// Verifies that an object reference is null.
        /// </summary>
        /// <param name="object">The object to be inspected</param>
        /// <exception cref="NullException">Thrown when the object reference is not null</exception>
        public static void Null(object @object)
        {
            if (@object != null)
                throw new NullException(@object);
        }

        /// <summary>
        /// Verifies that the provided object raised INotifyPropertyChanged.PropertyChanged
        /// as a result of executing the given test code.
        /// </summary>
        /// <param name="object">The object which should raise the notification</param>
        /// <param name="propertyName">The property name for which the notification should be raised</param>
        /// <param name="testCode">The test code which should cause the notification to be raised</param>
        /// <exception cref="PropertyChangedException">Thrown when the notification is not raised</exception>
        public static void PropertyChanged(INotifyPropertyChanged @object, string propertyName, Action testCode)
        {
            Guard.ArgumentNotNull("object", @object);
            Guard.ArgumentNotNull("testCode", testCode);

            bool propertyChangeHappened = false;

            PropertyChangedEventHandler handler = (sender, args) =>
            {
                if (propertyName.Equals(args.PropertyName, StringComparison.OrdinalIgnoreCase))
                    propertyChangeHappened = true;
            };

            @object.PropertyChanged += handler;

            try
            {
                testCode();
                if (!propertyChangeHappened)
                    throw new PropertyChangedException(propertyName);
            }
            finally
            {
                @object.PropertyChanged -= handler;
            }
        }

        /// <summary>
        /// Verifies that two objects are the same instance.
        /// </summary>
        /// <param name="expected">The expected object instance</param>
        /// <param name="actual">The actual object instance</param>
        /// <exception cref="SameException">Thrown when the objects are not the same instance</exception>
        public static void Same(object expected, object actual)
        {
            if (!object.ReferenceEquals(expected, actual))
                throw new SameException(expected, actual);
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

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// </summary>
        /// <typeparam name="T">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static T Throws<T>(Action testCode)
            where T : Exception
        {
            return (T)Throws(typeof(T), testCode);
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// Generally used to test property accessors.
        /// </summary>
        /// <typeparam name="T">The type of the exception expected to be thrown</typeparam>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static T Throws<T>(Func<object> testCode)
            where T : Exception
        {
            return (T)Throws(typeof(T), testCode);
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// </summary>
        /// <param name="exceptionType">The type of the exception expected to be thrown</param>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static Exception Throws(Type exceptionType, Action testCode)
        {
            Guard.ArgumentNotNull("exceptionType", exceptionType);

            Exception exception = Record.Exception(testCode);

            if (exception == null)
                throw new ThrowsException(exceptionType);

            if (!exceptionType.Equals(exception.GetType()))
                throw new ThrowsException(exceptionType, exception);

            return exception;
        }

        /// <summary>
        /// Verifies that the exact exception is thrown (and not a derived exception type).
        /// Generally used to test property accessors.
        /// </summary>
        /// <param name="exceptionType">The type of the exception expected to be thrown</param>
        /// <param name="testCode">A delegate to the code to be tested</param>
        /// <returns>The exception that was thrown, when successful</returns>
        /// <exception cref="ThrowsException">Thrown when an exception was not thrown, or when an exception of the incorrect type is thrown</exception>
        public static Exception Throws(Type exceptionType, Func<object> testCode)
        {
            Guard.ArgumentNotNull("exceptionType", exceptionType);

            Exception exception = Record.Exception(testCode);

            if (exception == null)
                throw new ThrowsException(exceptionType);

            if (!exceptionType.Equals(exception.GetType()))
                throw new ThrowsException(exceptionType, exception);

            return exception;
        }

        /// <summary>
        /// Verifies that an expression is true.
        /// </summary>
        /// <param name="condition">The condition to be inspected</param>
        /// <exception cref="TrueException">Thrown when the condition is false</exception>
        public static void True(bool condition)
        {
            True(condition, null);
        }

        /// <summary>
        /// Verifies that an expression is true.
        /// </summary>
        /// <param name="condition">The condition to be inspected</param>
        /// <param name="userMessage">The message to be shown when the condition is false</param>
        /// <exception cref="TrueException">Thrown when the condition is false</exception>
        public static void True(bool condition, string userMessage)
        {
            if (!condition)
                throw new TrueException(userMessage);
        }

        class AssertEqualityComparerAdapter<T> : IEqualityComparer
        {
            readonly IEqualityComparer<T> innerComparer;

            public AssertEqualityComparerAdapter(IEqualityComparer<T> innerComparer)
            {
                this.innerComparer = innerComparer;
            }

            public new bool Equals(object x, object y)
            {
                return innerComparer.Equals((T)x, (T)y);
            }

            public int GetHashCode(object obj)
            {
                throw new NotImplementedException();
            }
        }

        class AssertEqualityComparer<T> : IEqualityComparer<T>
        {
            static IEqualityComparer defaultInnerComparer = new AssertEqualityComparerAdapter<object>(new AssertEqualityComparer<object>());
            IEqualityComparer innerComparer;
            bool skipTypeCheck;

            public AssertEqualityComparer(bool skipTypeCheck = false, IEqualityComparer innerComparer = null)
            {
                this.skipTypeCheck = skipTypeCheck;
                this.innerComparer = innerComparer ?? defaultInnerComparer;
            }

            public bool Equals(T x, T y)
            {
                Type type = typeof(T);

                // Null?
                if (!type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>))))
                {
                    if (Object.Equals(x, default(T)))
                        return Object.Equals(y, default(T));

                    if (Object.Equals(y, default(T)))
                        return false;
                }

                // Same type?
                if (!skipTypeCheck && x.GetType() != y.GetType())
                    return false;

                // Implements IEquatable<T>?
                IEquatable<T> equatable = x as IEquatable<T>;
                if (equatable != null)
                    return equatable.Equals(y);

                // Implements IComparable<T>?
                IComparable<T> comparable1 = x as IComparable<T>;
                if (comparable1 != null)
                    return comparable1.CompareTo(y) == 0;

                // Implements IComparable?
                IComparable comparable2 = x as IComparable;
                if (comparable2 != null)
                    return comparable2.CompareTo(y) == 0;

                // Enumerable?
                IEnumerable enumerableX = x as IEnumerable;
                IEnumerable enumerableY = y as IEnumerable;

                if (enumerableX != null && enumerableY != null)
                {
                    IEnumerator enumeratorX = enumerableX.GetEnumerator();
                    IEnumerator enumeratorY = enumerableY.GetEnumerator();

                    while (true)
                    {
                        bool hasNextX = enumeratorX.MoveNext();
                        bool hasNextY = enumeratorY.MoveNext();

                        if (!hasNextX || !hasNextY)
                            return (hasNextX == hasNextY);

                        if (!innerComparer.Equals(enumeratorX.Current, enumeratorY.Current))
                            return false;
                    }
                }

                // Last case, rely on Object.Equals
                return Object.Equals(x, y);
            }

            public int GetHashCode(T obj)
            {
                throw new NotImplementedException();
            }
        }

        // Only used for Assert.InRange and Assert.NotInRange
        class AssertComparer<T> : IComparer<T> where T : IComparable
        {
            public int Compare(T x, T y)
            {
                Type type = typeof(T);

                // Null?
                if (!type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>))))
                {
                    if (Equals(x, default(T)))
                    {
                        if (Equals(y, default(T)))
                            return 0;
                        return -1;
                    }

                    if (Equals(y, default(T)))
                        return -1;
                }

                // Same type?
                if (x.GetType() != y.GetType())
                    return -1;

                // Implements IComparable<T>?
                IComparable<T> comparable1 = x as IComparable<T>;
                if (comparable1 != null)
                    return comparable1.CompareTo(y);

                // Implements IComparable
                return x.CompareTo(y);
            }
        }
    }
}