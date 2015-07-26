using System;
using System.Globalization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a string does not end with the expected value.
    /// </summary>
    public class EndsWithException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EndsWithException"/> class.
        /// </summary>
        /// <param name="expected">The expected string value</param>
        /// <param name="actual">The actual value</param>
        public EndsWithException(string expected, string actual)
            : base(string.Format(CultureInfo.CurrentCulture, "Assert.EndsWith() Failure:{2}Expected: {0}{2}Actual:   {1}", ShortenExpected(expected, actual) ?? "(null)", ShortenActual(expected, actual) ?? "(null)", Environment.NewLine))
        { }

        static string ShortenExpected(string expected, string actual)
        {
            if (expected == null || actual == null || actual.Length <= expected.Length)
                return expected;

            return "   " + expected;
        }

        static string ShortenActual(string expected, string actual)
        {
            if (expected == null || actual == null || actual.Length <= expected.Length)
                return actual;

            return "···" + actual.Substring(actual.Length - expected.Length);
        }
    }
}