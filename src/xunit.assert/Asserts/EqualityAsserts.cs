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
            Assert.GuardArgumentNotNull("comparer", comparer);

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
                    string.Format(CultureInfo.CurrentCulture, "{0} (rounded from {1})", expectedRounded, expected),
                    string.Format(CultureInfo.CurrentCulture, "{0} (rounded from {1})", actualRounded, actual)
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
                    string.Format(CultureInfo.CurrentCulture, "{0} (rounded from {1})", expectedRounded, expected),
                    string.Format(CultureInfo.CurrentCulture, "{0} (rounded from {1})", actualRounded, actual)
                );
        }

        /// <summary>
        /// Verifies that two objects are strictly equal, using the type's default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The value to be compared against</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal</exception>
        public static void StrictEqual<T>(T expected, T actual)
        {
            Equal(expected, actual, EqualityComparer<T>.Default);
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
            Assert.GuardArgumentNotNull("comparer", comparer);

            if (comparer.Equals(expected, actual))
                throw new NotEqualException(ArgumentFormatter.Format(expected), ArgumentFormatter.Format(actual));
        }

        /// <summary>
        /// Verifies that two <see cref="double"/> values are not equal, within the number of decimal
        /// places given by <paramref name="precision"/>.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="precision">The number of decimal places (valid values: 0-15)</param>
        /// <exception cref="EqualException">Thrown when the values are equal</exception>
        public static void NotEqual(double expected, double actual, int precision)
        {
            var expectedRounded = Math.Round(expected, precision);
            var actualRounded = Math.Round(actual, precision);

            if (GetEqualityComparer<double>().Equals(expectedRounded, actualRounded))
                throw new NotEqualException(
                    string.Format(CultureInfo.CurrentCulture, "{0} (rounded from {1})", expectedRounded, expected),
                    string.Format(CultureInfo.CurrentCulture, "{0} (rounded from {1})", actualRounded, actual)
                );
        }

        /// <summary>
        /// Verifies that two <see cref="decimal"/> values are not equal, within the number of decimal
        /// places given by <paramref name="precision"/>.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="precision">The number of decimal places (valid values: 0-15)</param>
        /// <exception cref="EqualException">Thrown when the values are equal</exception>
        public static void NotEqual(decimal expected, decimal actual, int precision)
        {
            var expectedRounded = Math.Round(expected, precision);
            var actualRounded = Math.Round(actual, precision);

            if (GetEqualityComparer<decimal>().Equals(expectedRounded, actualRounded))
                throw new NotEqualException(
                    string.Format(CultureInfo.CurrentCulture, "{0} (rounded from {1})", expectedRounded, expected),
                    string.Format(CultureInfo.CurrentCulture, "{0} (rounded from {1})", actualRounded, actual)
                );
        }

        /// <summary>
        /// Verifies that two objects are strictly not equal, using the type's default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The actual object</param>
        /// <exception cref="NotEqualException">Thrown when the objects are equal</exception>
        public static void NotStrictEqual<T>(T expected, T actual)
        {
            NotEqual(expected, actual, EqualityComparer<T>.Default);
        }
    }
}
