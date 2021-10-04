using System;
using System.Text.RegularExpressions;
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
			Assert.Equal(
				"Assert.Contains() Failure" + Environment.NewLine +
				"Not found: WORLD" + Environment.NewLine +
				"In value:  Hello, world!",
				ex.Message
			);
		}

		[Fact]
		public void SubstringNotFound()
		{
			Assert.Throws<ContainsException>(() => Assert.Contains("hey", "Hello, world!"));
		}

		[Fact]
		public void NullActualStringThrows()
		{
			Assert.Throws<ContainsException>(() => Assert.Contains("foo", (string?)null));
		}
	}

	public class Contains_WithComparisonType
	{
		[Fact]
		public void CanSearchForSubstringsCaseInsensitive()
		{
			Assert.Contains("WORLD", "Hello, world!", StringComparison.OrdinalIgnoreCase);
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
			var ex = Record.Exception(() => Assert.DoesNotContain("world", "Hello, world!"));

			Assert.IsType<DoesNotContainException>(ex);
			Assert.Equal(
				"Assert.DoesNotContain() Failure" + Environment.NewLine +
				"Found:    world" + Environment.NewLine +
				"In value: Hello, world!",
				ex.Message
			);
		}

		[Fact]
		public void NullActualStringDoesNotThrow()
		{
			Assert.DoesNotContain("foo", (string?)null);
		}
	}

	public class DoesNotContain_WithComparisonType
	{
		[Fact]
		public void CanSearchForSubstringsCaseInsensitive()
		{
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain("WORLD", "Hello, world!", StringComparison.OrdinalIgnoreCase));
		}
	}

	public class Equal
	{
		[Theory]
		// Null values
		[InlineData(null, null, false, false, false)]
		// Empty values
		[InlineData("", "", false, false, false)]
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
#if XUNIT_SPAN
		[InlineData(" ", " \u180E", false, false, true)]
		[InlineData(" \u180E", "\u180E ", false, false, true)]
		[InlineData("    ", "\u180E", false, false, true)]
		[InlineData(" ", " \u200B", false, false, true)]
		[InlineData(" \u200B", "\u200B ", false, false, true)]
		[InlineData("    ", "\u200B", false, false, true)]
		[InlineData(" ", " \u200B\uFEFF", false, false, true)]
		[InlineData(" \u180E", "\u200B\u202F\u1680\u180E ", false, false, true)]
		[InlineData("\u2001\u2002\u2003\u2006\u2009    ", "\u200B", false, false, true)]
		[InlineData("\u00A0\u200A\u2009\u2006\u2009    ", "\u200B", false, false, true)]
		// The ogham space mark (\u1680) kind of looks like a faint dash, but Microsoft has put it
		// inside the SpaceSeparator unicode category, so we also treat it as a space
		[InlineData("\u2007\u2008\u1680\t\u0009\u3000   ", " ", false, false, true)]
		[InlineData("\u1680", "\t", false, false, true)]
		[InlineData("\u1680", "       ", false, false, true)]
#endif
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
		public void FailureCases(string? expected, string? actual, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected, actual, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences)
			);

			var eqEx = Assert.IsType<EqualException>(ex);
			Assert.Equal(expectedIndex, eqEx.ExpectedIndex);
			Assert.Equal(actualIndex, eqEx.ActualIndex);
		}

		[Fact]
		public void MessageFormatting()
		{
			var ex = Record.Exception(() =>
				Assert.Equal(
					"Why hello there world, you're a long string with some truncation!",
					"Why hello there world! You're a long string!"
				)
			);

			Assert.IsType<EqualException>(ex);
			Assert.Equal(
				"Assert.Equal() Failure" + Environment.NewLine +
				"                                 ↓ (pos 21)" + Environment.NewLine +
				"Expected: ···hy hello there world, you're a long string with some truncati···" + Environment.NewLine +
				"Actual:   ···hy hello there world! You're a long string!" + Environment.NewLine +
				"                                 ↑ (pos 21)",
				ex.Message
			);
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
			Assert.Equal(
				"Assert.StartsWith() Failure:" + Environment.NewLine +
				"Expected: HELLO" + Environment.NewLine +
				"Actual:   Hello",
				ex.Message
			);
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
			Assert.StartsWith("HELLO", "Hello, world!", StringComparison.OrdinalIgnoreCase);
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
			Assert.Equal(
				"Assert.EndsWith() Failure:" + Environment.NewLine +
				"Expected: WORLD!" + Environment.NewLine +
				"Actual:   world!",
				ex.Message
			);
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
			Assert.EndsWith("WORLD!", "Hello, world!", StringComparison.OrdinalIgnoreCase);
		}
	}

	public class Matches_WithString
	{
		[Fact]
		public void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>(() => Assert.Matches((string?)null!, "Hello, world!"));
			Assert.Throws<MatchesException>(() => Assert.Matches(@"\w+", null));
		}

		[Fact]
		public void Success()
		{
			Assert.Matches(@"\w", "Hello");
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.Matches(@"\d+", "Hello, world!"));

			Assert.IsType<MatchesException>(ex);
			Assert.Equal(
				"Assert.Matches() Failure:" + Environment.NewLine +
				@"Regex: \d+" + Environment.NewLine +
				"Value: Hello, world!",
				ex.Message
			);
		}
	}

	public class Matches_WithRegex
	{
		[Fact]
		public void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>(() => Assert.Matches((Regex?)null!, "Hello, world!"));
			Assert.Throws<MatchesException>(() => Assert.Matches(new Regex(@"\w+"), null));
		}

		[Fact]
		public void Success()
		{
			Assert.Matches(new Regex(@"\w+"), "Hello");
		}

		[Fact]
		public void UsesRegexOptions()
		{
			Assert.Matches(new Regex(@"[a-z]+", RegexOptions.IgnoreCase), "HELLO");
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.Matches(new Regex(@"\d+"), "Hello, world!"));

			Assert.IsType<MatchesException>(ex);
			Assert.Equal(
				"Assert.Matches() Failure:" + Environment.NewLine +
				@"Regex: \d+" + Environment.NewLine +
				"Value: Hello, world!",
				ex.Message
			);
		}
	}

	public class DoesNotMatch_WithString
	{
		[Fact]
		public void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>(() => Assert.DoesNotMatch((string?)null!, "Hello, world!"));
			Assert.DoesNotMatch(@"\w+", null);
		}

		[Fact]
		public void Success()
		{
			Assert.DoesNotMatch(@"\d", "Hello");
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.DoesNotMatch(@"\w", "Hello, world!"));

			Assert.IsType<DoesNotMatchException>(ex);
			Assert.Equal(
				"Assert.DoesNotMatch() Failure:" + Environment.NewLine +
				@"Regex: \w" + Environment.NewLine +
				"Value: Hello, world!",
				ex.Message
			);
		}
	}

	public class DoesNotMatch_WithRegex
	{
		[Fact]
		public void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>(() => Assert.DoesNotMatch((Regex?)null!, "Hello, world!"));
			Assert.DoesNotMatch(new Regex(@"\w+"), null);
		}

		[Fact]
		public void Success()
		{
			Assert.DoesNotMatch(new Regex(@"\d"), "Hello");
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.DoesNotMatch(new Regex(@"\w"), "Hello, world!"));

			Assert.IsType<DoesNotMatchException>(ex);
			Assert.Equal(
				"Assert.DoesNotMatch() Failure:" + Environment.NewLine +
				@"Regex: \w" + Environment.NewLine +
				"Value: Hello, world!",
				ex.Message
			);
		}
	}
}
