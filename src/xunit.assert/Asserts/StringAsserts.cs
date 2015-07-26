using System;
using System.Text.RegularExpressions;
using Xunit.Sdk;

namespace Xunit
{
    public partial class Assert
    {
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
                throw new DoesNotContainException(expectedSubstring, actualString);
        }

        /// <summary>
        /// Verifies that a string starts with a given string, using the current culture.
        /// </summary>
        /// <param name="expectedStartString">The string expected to be at the start of the string</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <exception cref="ContainsException">Thrown when the string does not start with the expected string</exception>
        public static void StartsWith(string expectedStartString, string actualString)
        {
            StartsWith(expectedStartString, actualString, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Verifies that a string starts with a given string, using the given comparison type.
        /// </summary>
        /// <param name="expectedStartString">The string expected to be at the start of the string</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <param name="comparisonType">The type of string comparison to perform</param>
        /// <exception cref="ContainsException">Thrown when the string does not start with the expected string</exception>
        public static void StartsWith(string expectedStartString, string actualString, StringComparison comparisonType)
        {
            if (expectedStartString == null || actualString == null || !actualString.StartsWith(expectedStartString, comparisonType))
                throw new StartsWithException(expectedStartString, actualString);
        }

        /// <summary>
        /// Verifies that a string ends with a given string, using the current culture.
        /// </summary>
        /// <param name="expectedEndString">The string expected to be at the end of the string</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <exception cref="ContainsException">Thrown when the string does not end with the expected string</exception>
        public static void EndsWith(string expectedEndString, string actualString)
        {
            EndsWith(expectedEndString, actualString, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Verifies that a string ends with a given string, using the given comparison type.
        /// </summary>
        /// <param name="expectedEndString">The string expected to be at the end of the string</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <param name="comparisonType">The type of string comparison to perform</param>
        /// <exception cref="ContainsException">Thrown when the string does not end with the expected string</exception>
        public static void EndsWith(string expectedEndString, string actualString, StringComparison comparisonType)
        {
            if (expectedEndString == null || actualString == null || !actualString.EndsWith(expectedEndString, comparisonType))
                throw new EndsWithException(expectedEndString, actualString);
        }

        /// <summary>
        /// Verifies that a string matches a regular expression.
        /// </summary>
        /// <param name="expectedRegexPattern">The regex pattern expected to match</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <exception cref="MatchesException">Thrown when the string does not match the regex pattern</exception>
        public static void Matches(string expectedRegexPattern, string actualString)
        {
            Assert.GuardArgumentNotNull("expectedRegexPattern", expectedRegexPattern);

            if (actualString == null || !Regex.IsMatch(actualString, expectedRegexPattern))
                throw new MatchesException(expectedRegexPattern, actualString);
        }

        /// <summary>
        /// Verifies that a string matches a regular expression.
        /// </summary>
        /// <param name="expectedRegex">The regex expected to match</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <exception cref="MatchesException">Thrown when the string does not match the regex</exception>
        public static void Matches(Regex expectedRegex, string actualString)
        {
            Assert.GuardArgumentNotNull("expectedRegex", expectedRegex);

            if (actualString == null || !expectedRegex.IsMatch(actualString))
                throw new MatchesException(expectedRegex.ToString(), actualString);
        }

        /// <summary>
        /// Verifies that a string does not match a regular expression.
        /// </summary>
        /// <param name="expectedRegexPattern">The regex pattern expected not to match</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <exception cref="DoesNotMatchException">Thrown when the string matches the regex pattern</exception>
        public static void DoesNotMatch(string expectedRegexPattern, string actualString)
        {
            Assert.GuardArgumentNotNull("expectedRegexPattern", expectedRegexPattern);

            if (actualString != null && Regex.IsMatch(actualString, expectedRegexPattern))
                throw new DoesNotMatchException(expectedRegexPattern, actualString);
        }

        /// <summary>
        /// Verifies that a string does not match a regular expression.
        /// </summary>
        /// <param name="expectedRegex">The regex expected not to match</param>
        /// <param name="actualString">The string to be inspected</param>
        /// <exception cref="DoesNotMatchException">Thrown when the string matches the regex</exception>
        public static void DoesNotMatch(Regex expectedRegex, string actualString)
        {
            Assert.GuardArgumentNotNull("expectedRegex", expectedRegex);

            if (actualString != null && expectedRegex.IsMatch(actualString))
                throw new DoesNotMatchException(expectedRegex.ToString(), actualString);
        }

        /// <summary>
        /// Verifies that two strings are equivalent.
        /// </summary>
        /// <param name="expected">The expected string value.</param>
        /// <param name="actual">The actual string value.</param>
        /// <exception cref="EqualException">Thrown when the strings are not equivalent.</exception>
        public static void Equal(string expected, string actual)
        {
            Equal(expected, actual, false, false, false);
        }

        /// <summary>
        /// Verifies that two strings are equivalent.
        /// </summary>
        /// <param name="expected">The expected string value.</param>
        /// <param name="actual">The actual string value.</param>
        /// <param name="ignoreCase">If set to <c>true</c>, ignores cases differences. The invariant culture is used.</param>
        /// <param name="ignoreLineEndingDifferences">If set to <c>true</c>, treats \r\n, \r, and \n as equivalent.</param>
        /// <param name="ignoreWhiteSpaceDifferences">If set to <c>true</c>, treats spaces and tabs (in any non-zero quantity) as equivalent.</param>
        /// <exception cref="EqualException">Thrown when the strings are not equivalent.</exception>
        public static void Equal(string expected, string actual, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
        {
            // Start out assuming the one of the values is null
            int expectedIndex = -1;
            int actualIndex = -1;
            int expectedLength = 0;
            int actualLength = 0;

            if (expected == null)
            {
                if (actual == null)
                    return;
            }
            else if (actual != null)
            {
                // Walk the string, keeping separate indices since we can skip variable amounts of
                // data based on ignoreLineEndingDifferences and ignoreWhiteSpaceDifferences.
                expectedIndex = 0;
                actualIndex = 0;
                expectedLength = expected.Length;
                actualLength = actual.Length;

                while (expectedIndex < expectedLength && actualIndex < actualLength)
                {
                    char expectedChar = expected[expectedIndex];
                    char actualChar = actual[actualIndex];

                    if (ignoreLineEndingDifferences && IsLineEnding(expectedChar) && IsLineEnding(actualChar))
                    {
                        expectedIndex = SkipLineEnding(expected, expectedIndex);
                        actualIndex = SkipLineEnding(actual, actualIndex);
                    }
                    else if (ignoreWhiteSpaceDifferences && IsWhiteSpace(expectedChar) && IsWhiteSpace(actualChar))
                    {
                        expectedIndex = SkipWhitespace(expected, expectedIndex);
                        actualIndex = SkipWhitespace(actual, actualIndex);
                    }
                    else
                    {
                        if (ignoreCase)
                        {
                            expectedChar = Char.ToUpperInvariant(expectedChar);
                            actualChar = Char.ToUpperInvariant(actualChar);
                        }

                        if (expectedChar != actualChar)
                        {
                            break;
                        }

                        expectedIndex++;
                        actualIndex++;
                    }
                }
            }

            if (expectedIndex < expectedLength || actualIndex < actualLength)
            {
                throw new EqualException(expected, actual, expectedIndex, actualIndex);
            }
        }

        static bool IsLineEnding(char c)
        {
            return c == '\r' || c == '\n';
        }

        static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t';
        }

        static int SkipLineEnding(string value, int index)
        {
            if (value[index] == '\r')
            {
                ++index;
            }
            if (index < value.Length && value[index] == '\n')
            {
                ++index;
            }

            return index;
        }

        static int SkipWhitespace(string value, int index)
        {
            while (index < value.Length)
            {
                switch (value[index])
                {
                    case ' ':
                    case '\t':
                        index++;
                        break;

                    default:
                        return index;
                }
            }

            return index;
        }
    }
}