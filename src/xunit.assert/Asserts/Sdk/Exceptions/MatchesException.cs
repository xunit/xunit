using System;
using System.Globalization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a string does not match a regular expression.
    /// </summary>
    public class MatchesException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MatchesException"/> class.
        /// </summary>
        /// <param name="expectedRegexPattern">The expected regular expression pattern</param>
        /// <param name="actual">The actual value</param>
        public MatchesException(object expectedRegexPattern, object actual)
            : base(string.Format(CultureInfo.CurrentCulture, "Assert.Matches() Failure:{2}Regex: {0}{2}Value: {1}", expectedRegexPattern, actual, Environment.NewLine))
        { }
    }
}