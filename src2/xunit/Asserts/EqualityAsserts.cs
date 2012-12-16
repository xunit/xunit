using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit.Sdk;

namespace Xunit
{
    public partial class Assert
    {
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
                    String.Format(CultureInfo.CurrentCulture, "{0} (rounded from {1})", actualRounded, actual)
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
                    String.Format(CultureInfo.CurrentCulture, "{0} (rounded from {1})", actualRounded, actual)
                );
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
    }
}