namespace Xunit.Sdk
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Exception thrown when a string does not match a regular expression.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class MatchesException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MatchesException"/> class.
        /// </summary>
        /// <param name="expectedRegexPattern">The expected regular expression pattern</param>
        /// <param name="actual">The actual value</param>
        public MatchesException(object expectedRegexPattern, object actual)
            : base(String.Format(CultureInfo.CurrentCulture, "Assert.Matches() Failure:{2}Regex: {0}{2}Value: {1}", expectedRegexPattern, actual, Environment.NewLine)) { }
    }
}