﻿using System;
using Xunit;
using Xunit.Sdk;

public class StringAssertsTests
{
    public class Contains
    {
        [Fact]
        public void CanSearchForSubstrings()
        {
            Assert.Contains("wor", "Hello, world!");
        }

        [Fact]
        public void SubstringContainsIsCaseSensitiveByDefault()
        {
            var ex = Record.Exception(() => Assert.Contains("WORLD", "Hello, world!"));

            Assert.IsType<ContainsException>(ex);
            Assert.Equal("Assert.Contains() Failure:" + Environment.NewLine +
                         "Not found: WORLD" + Environment.NewLine +
                         "In value:  Hello, world!", ex.Message);
        }

        [Fact]
        public void SubstringNotFound()
        {
            Assert.Throws<ContainsException>(() => Assert.Contains("hey", "Hello, world!"));
        }

        [Fact]
        public void NullActualStringThrows()
        {
            Assert.Throws<ContainsException>(() => Assert.Contains("foo", (string)null));
        }
    }

    public class Contains_WithComparisonType
    {
        [Fact]
        public void CanSearchForSubstringsCaseInsensitive()
        {
            Assert.Contains("WORLD", "Hello, world!", StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public class DoesNotContain
    {
        [Fact]
        public void CanSearchForSubstrings()
        {
            Assert.DoesNotContain("hey", "Hello, world!");
        }

        [Fact]
        public void SubstringDoesNotContainIsCaseSensitiveByDefault()
        {
            Assert.DoesNotContain("WORLD", "Hello, world!");
        }

        [Fact]
        public void SubstringFound()
        {
            Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain("world", "Hello, world!"));
        }

        [Fact]
        public void NullActualStringDoesNotThrow()
        {
            Assert.DoesNotThrow(() => Assert.DoesNotContain("foo", (string)null));
        }
    }

    public class DoesNotContain_WithComparisonType
    {
        [Fact]
        public void CanSearchForSubstringsCaseInsensitive()
        {
            Assert.Throws<DoesNotContainException>(
                () => Assert.DoesNotContain("WORLD", "Hello, world!", StringComparison.InvariantCultureIgnoreCase));
        }
    }

    public class Equal
    {
        [Theory]
        // Null values
        [InlineData(null, null, false, false, false)]
        // Identical values
        [InlineData("foo", "foo", false, false, false)]
        // Case differences
        [InlineData("foo", "FoO", true, false, false)]
        // Line ending differences
        [InlineData("foo \r\n bar", "foo \r bar", false, true, false)]
        [InlineData("foo \r\n bar", "foo \n bar", false, true, false)]
        [InlineData("foo \n bar", "foo \r bar", false, true, false)]
        // Whitespace differences
        [InlineData(" ", "\t", false, false, true)]
        [InlineData(" \t", "\t ", false, false, true)]
        [InlineData("    ", "\t", false, false, true)]
        public void SuccessCases(string value1, string value2, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences)
        {
            // Run them in both directions, as the values should be interchangeable when they're equal
            Assert.Equal(value1, value2, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);
            Assert.Equal(value2, value1, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);
        }

        [Theory]
        // Null values
        [InlineData(null, "", false, false, false, -1, -1)]
        [InlineData("", null, false, false, false, -1, -1)]
        // Non-identical values
        [InlineData("foo", "foo!", false, false, false, 3, 3)]
        [InlineData("foo", "foo\0", false, false, false, 3, 3)]
        // Case differences
        [InlineData("foo bar", "foo   Bar", false, true, true, 4, 6)]
        // Line ending differences
        [InlineData("foo \nbar", "FoO  \rbar", true, false, true, 4, 5)]
        // Whitespace differences
        [InlineData("foo\n bar", "FoO\r\n  bar", true, true, false, 5, 6)]
        public void FailureCases(string expected, string actual, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences, int expectedIndex, int actualIndex)
        {
            Exception ex = Record.Exception(
                () => Assert.Equal(expected, actual, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences)
            );

            EqualException eqEx = Assert.IsType<EqualException>(ex);
            Assert.Equal(expectedIndex, eqEx.ExpectedIndex);
            Assert.Equal(actualIndex, eqEx.ActualIndex);
        }
    }

    public class StartsWith
    {
        [Fact]
        public void Success()
        {
            Assert.StartsWith("Hello", "Hello, world!");
        }

        [Fact]
        public void IsCaseSensitiveByDefault()
        {
            var ex = Record.Exception(() => Assert.StartsWith("HELLO", "Hello"));

            Assert.IsType<StartsWithException>(ex);
            Assert.Equal("Assert.StartsWith() Failure:" + Environment.NewLine +
                         "Expected: HELLO" + Environment.NewLine +
                         "Actual:   Hello", ex.Message);
        }

        [Fact]
        public void NotFound()
        {
            Assert.Throws<StartsWithException>(() => Assert.StartsWith("hey", "Hello, world!"));
        }

        [Fact]
        public void NullActualStringThrows()
        {
            Assert.Throws<StartsWithException>(() => Assert.StartsWith("foo", null));
        }
    }

    public class StartsWith_WithComparisonType
    {
        [Fact]
        public void CanSearchForSubstringsCaseInsensitive()
        {
            Assert.StartsWith("HELLO", "Hello, world!", StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public class EndsWith
    {
        [Fact]
        public void Success()
        {
            Assert.EndsWith("world!", "Hello, world!");
        }

        [Fact]
        public void IsCaseSensitiveByDefault()
        {
            var ex = Record.Exception(() => Assert.EndsWith("WORLD!", "world!"));

            Assert.IsType<EndsWithException>(ex);
            Assert.Equal("Assert.EndsWith() Failure:" + Environment.NewLine +
                         "Expected: WORLD!" + Environment.NewLine +
                         "Actual:   world!", ex.Message);
        }

        [Fact]
        public void NotFound()
        {
            Assert.Throws<EndsWithException>(() => Assert.EndsWith("hey", "Hello, world!"));
        }

        [Fact]
        public void NullActualStringThrows()
        {
            Assert.Throws<EndsWithException>(() => Assert.EndsWith("foo", null));
        }
    }

    public class EndsWith_WithComparisonType
    {
        [Fact]
        public void CanSearchForSubstringsCaseInsensitive()
        {
            Assert.EndsWith("WORLD!", "Hello, world!", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}