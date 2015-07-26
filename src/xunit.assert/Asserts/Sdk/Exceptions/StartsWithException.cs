using System;
using System.Globalization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a string does not start with the expected value.
    /// </summary>
    public class StartsWithException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="StartsWithException"/> class.
        /// </summary>
        /// <param name="expected">The expected string value</param>
        /// <param name="actual">The actual value</param>
        public StartsWithException(string expected, string actual)
            : base(string.Format(CultureInfo.CurrentCulture, "Assert.StartsWith() Failure:{2}Expected: {0}{2}Actual:   {1}", expected ?? "(null)", ShortenActual(expected, actual) ?? "(null)", Environment.NewLine))
        { }

        static string ShortenActual(string expected, string actual)
        {
            if (expected == null || actual == null || actual.Length <= expected.Length)
            {
                return actual;
            }

            return actual.Substring(0, expected.Length) + "...";
        }
    }
}