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
				"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
				"String:    \"Hello, world!\"" + Environment.NewLine +
				"Not found: \"WORLD\"",
				ex.Message
			);
		}

		[Fact]
		public void SubstringNotFound()
		{
			var ex = Record.Exception(() => Assert.Contains("hey", "Hello, world!"));

			Assert.IsType<ContainsException>(ex);
			Assert.Equal(
				"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
				"String:    \"Hello, world!\"" + Environment.NewLine +
				"Not found: \"hey\"",
				ex.Message
			);
		}

		[Fact]
		public void NullActualStringThrows()
		{
			var ex = Record.Exception(() => Assert.Contains("foo", default(string)));

			Assert.IsType<ContainsException>(ex);
			Assert.Equal(
				"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
				"String:    null" + Environment.NewLine +
				"Not found: \"foo\"",
				ex.Message
			);
		}

		[Fact]
		public void VeryLongStrings()
		{
			var ex = Record.Exception(
				() => Assert.Contains(
					"We are looking for something very long as well",
					"This is a relatively long string so that we can see the truncation in action"
				)
			);

			Assert.IsType<ContainsException>(ex);
			Assert.Equal(
				"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
				$"String:    \"This is a relatively long string so that \"{ArgumentFormatter.Ellipsis}" + Environment.NewLine +
				$"Not found: \"We are looking for something very long as\"{ArgumentFormatter.Ellipsis}",
				ex.Message
			);
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
				"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
				"                ↓ (pos 7)" + Environment.NewLine +
				"String: \"Hello, world!\"" + Environment.NewLine +
				"Found:  \"world\"",
				ex.Message
			);
		}

		[Fact]
		public void NullActualStringDoesNotThrow()
		{
			Assert.DoesNotContain("foo", (string?)null);
		}

		[Fact]
		public void VeryLongString_FoundAtFront()
		{
			var ex = Record.Exception(() => Assert.DoesNotContain("world", "Hello, world from a very long string that will end up being truncated"));

			Assert.IsType<DoesNotContainException>(ex);
			Assert.Equal(
				"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
				"                ↓ (pos 7)" + Environment.NewLine +
				$"String: \"Hello, world from a very long string that\"{ArgumentFormatter.Ellipsis}" + Environment.NewLine +
				"Found:  \"world\"",
				ex.Message
			);
		}

		[Fact]
		public void VeryLongString_FoundInMiddle()
		{
			var ex = Record.Exception(() => Assert.DoesNotContain("world", "This is a relatively long string that has 'Hello, world' placed in the middle so that we can dual trunaction"));

			Assert.IsType<DoesNotContainException>(ex);
			Assert.Equal(
				"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
				"                                ↓ (pos 50)" + Environment.NewLine +
				$"String: {ArgumentFormatter.Ellipsis}\"ng that has 'Hello, world' placed in the \"{ArgumentFormatter.Ellipsis}" + Environment.NewLine +
				"Found:  \"world\"",
				ex.Message
			);
		}

		[Fact]
		public void VeryLongString_FoundAtEnd()
		{
			var ex = Record.Exception(() => Assert.DoesNotContain("world", "This is a relatively long string that will from the front truncated, just to say 'Hello, world'"));

			Assert.IsType<DoesNotContainException>(ex);
			Assert.Equal(
				"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
				"                                               ↓ (pos 89)" + Environment.NewLine +
				$"String: {ArgumentFormatter.Ellipsis}\"ont truncated, just to say 'Hello, world'\"" + Environment.NewLine +
				"Found:  \"world\"",
				ex.Message
			);
		}
	}

	public class DoesNotContain_WithComparisonType
	{
		[Fact]
		public void CanSearchForSubstringsCaseInsensitive()
		{
			var ex = Record.Exception(() => Assert.DoesNotContain("WORLD", "Hello, world!", StringComparison.OrdinalIgnoreCase));

			Assert.IsType<DoesNotContainException>(ex);
			Assert.Equal(
				"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
				"                ↓ (pos 7)" + Environment.NewLine +
				"String: \"Hello, world!\"" + Environment.NewLine +
				"Found:  \"WORLD\"",
				ex.Message
			);
		}
	}

	public class DoesNotMatch_Pattern
	{
		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("expectedRegexPattern", () => Assert.DoesNotMatch((string?)null!, "Hello, world!"));
		}

		[Fact]
		public void Success()
		{
			Assert.DoesNotMatch(@"\d", "Hello");
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.DoesNotMatch("ll", "Hello, world!"));

			Assert.IsType<DoesNotMatchException>(ex);
			Assert.Equal(
				"Assert.DoesNotMatch() Failure: Match found" + Environment.NewLine +
				"           ↓ (pos 2)" + Environment.NewLine +
				"String: \"Hello, world!\"" + Environment.NewLine +
				"RegEx:  \"ll\"",
				ex.Message
			);
		}
	}

	public class DoesNotMatch_Regex
	{
		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("expectedRegex", () => Assert.DoesNotMatch((Regex?)null!, "Hello, world!"));
		}

		[Fact]
		public void Success()
		{
			Assert.DoesNotMatch(new Regex(@"\d"), "Hello");
		}

		[Fact]
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.DoesNotMatch(new Regex(@"ll"), "Hello, world!"));

			Assert.IsType<DoesNotMatchException>(ex);
			Assert.Equal(
				"Assert.DoesNotMatch() Failure: Match found" + Environment.NewLine +
				"           ↓ (pos 2)" + Environment.NewLine +
				"String: \"Hello, world!\"" + Environment.NewLine +
				"RegEx:  \"ll\"",
				ex.Message
			);
		}
	}

	public class Empty
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("value", () => Assert.Empty(default(string)!));
		}

		[Fact]
		public static void EmptyString()
		{
			Assert.Empty("");
		}

		[Fact]
		public static void NonEmptyString()
		{
			var ex = Record.Exception(() => Assert.Empty("Foo"));

			Assert.IsType<EmptyException>(ex);
			Assert.Equal(
				"Assert.Empty() Failure: String was not empty" + Environment.NewLine +
				"String: \"Foo\"",
				ex.Message
			);
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
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.EndsWith("hey", "Hello, world!"));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       \"Hello, world!\"" + Environment.NewLine +
				"Expected end: \"hey\"",
				ex.Message
			);
		}

		[Fact]
		public void CaseSensitiveByDefault()
		{
			var ex = Record.Exception(() => Assert.EndsWith("WORLD!", "world!"));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       \"world!\"" + Environment.NewLine +
				"Expected end: \"WORLD!\"",
				ex.Message
			);
		}

		[Fact]
		public void CanSpecifyComparisonType()
		{
			Assert.EndsWith("WORLD!", "Hello, world!", StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void NullString()
		{
			var ex = Record.Exception(() => Assert.EndsWith("foo", null));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       null" + Environment.NewLine +
				"Expected end: \"foo\"",
				ex.Message
			);
		}

		[Fact]
		public void Truncation()
		{
			var expected = "This is a long string that we're looking for at the end";
			var actual = "This is the long string that we expected to find this ending inside";

			var ex = Record.Exception(() => Assert.EndsWith(expected, actual));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       " + ArgumentFormatter.Ellipsis + "\"at we expected to find this ending inside\"" + Environment.NewLine +
				"Expected end: \"This is a long string that we're looking \"" + ArgumentFormatter.Ellipsis,
				ex.Message
			);
		}
	}

	public class Equal
	{
		[Theory]
		// Null values
		[InlineData(null, null, false, false, false, false)]
		// Empty values
		[InlineData("", "", false, false, false, false)]
		// Identical values
		[InlineData("foo", "foo", false, false, false, false)]
		// Case differences
		[InlineData("foo", "FoO", true, false, false, false)]
		// Line ending differences
		[InlineData("foo \r\n bar", "foo \r bar", false, true, false, false)]
		[InlineData("foo \r\n bar", "foo \n bar", false, true, false, false)]
		[InlineData("foo \n bar", "foo \r bar", false, true, false, false)]
		// Whitespace differences
		[InlineData(" ", "\t", false, false, true, false)]
		[InlineData(" \t", "\t ", false, false, true, false)]
		[InlineData("    ", "\t", false, false, true, false)]
#if XUNIT_SPAN
		[InlineData(" ", " \u180E", false, false, true, false)]
		[InlineData(" \u180E", "\u180E ", false, false, true, false)]
		[InlineData("    ", "\u180E", false, false, true, false)]
		[InlineData(" ", " \u200B", false, false, true, false)]
		[InlineData(" \u200B", "\u200B ", false, false, true, false)]
		[InlineData("    ", "\u200B", false, false, true, false)]
		[InlineData(" ", " \u200B\uFEFF", false, false, true, false)]
		[InlineData(" \u180E", "\u200B\u202F\u1680\u180E ", false, false, true, false)]
		[InlineData("\u2001\u2002\u2003\u2006\u2009    ", "\u200B", false, false, true, false)]
		[InlineData("\u00A0\u200A\u2009\u2006\u2009    ", "\u200B", false, false, true, false)]
		// The ogham space mark (\u1680) kind of looks like a faint dash, but Microsoft has put it
		// inside the SpaceSeparator unicode category, so we also treat it as a space
		[InlineData("\u2007\u2008\u1680\t\u0009\u3000   ", " ", false, false, true, false)]
		[InlineData("\u1680", "\t", false, false, true, false)]
		[InlineData("\u1680", "       ", false, false, true, false)]
#endif
		// All whitespace differences
		[InlineData("", "  ", false, false, false, true)]
		[InlineData("", "  ", false, false, true, true)]
		[InlineData("", "\t", false, false, true, true)]
		[InlineData("foobar", "foo bar", false, false, true, true)]
		public void Success(
			string? value1,
			string? value2,
			bool ignoreCase,
			bool ignoreLineEndingDifferences,
			bool ignoreWhiteSpaceDifferences,
			bool ignoreAllWhiteSpace)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1, value2, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value2, value1, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
		}

		[Theory]
		// Non-identical values
		[InlineData("foo", "foo!", false, false, false, false, null, "   ↑ (pos 3)")]
		[InlineData("foo\0", "foo\0\0", false, false, false, false, null, "     ↑ (pos 4)")]
		// Nulls
		[InlineData("first test 1", null, false, false, false, false, null, null)]
		[InlineData(null, "first test 1", false, false, false, false, null, null)]
		// Overruns
		[InlineData("first test", "first test 1", false, false, false, false, null, "          ↑ (pos 10)")]
		[InlineData("first test 1", "first test", false, false, false, false, "          ↓ (pos 10)", null)]
		// Case differences
		[InlineData("Foobar", "foo bar", true, false, false, false, "   ↓ (pos 3)", "   ↑ (pos 3)")]
		// Line ending differences
		[InlineData("foo\nbar", "foo\rBar", false, true, false, false, "     ↓ (pos 4)", "     ↑ (pos 4)")]
		// Non-zero whitespace quantity differences
		[InlineData("foo bar", "foo  Bar", false, false, true, false, "    ↓ (pos 4)", "     ↑ (pos 5)")]
		// Ignore all white space differences
		[InlineData("foobar", "foo Bar", false, false, false, true, "   ↓ (pos 3)", "    ↑ (pos 4)")]
		public void Failure(
			string? expected,
			string? actual,
			bool ignoreCase,
			bool ignoreLineEndingDifferences,
			bool ignoreWhiteSpaceDifferences,
			bool ignoreAllWhiteSpace,
			string? expectedPointer,
			string? actualPointer)
		{
			var message = "Assert.Equal() Failure: Strings differ";

			if (expectedPointer is not null)
				message += Environment.NewLine + "           " + expectedPointer;

			message +=
				Environment.NewLine + "Expected: " + ArgumentFormatter.Format(expected) +
				Environment.NewLine + "Actual:   " + ArgumentFormatter.Format(actual);

			if (actualPointer is not null)
				message += Environment.NewLine + "           " + actualPointer;

			var ex = Record.Exception(
				() => Assert.Equal(expected, actual, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace)
			);

			Assert.IsType<EqualException>(ex);
			Assert.Equal<object>(
				message,
				ex.Message
			);
		}

		[Fact]
		public void Truncation()
		{
			var ex = Record.Exception(() =>
				Assert.Equal(
					"Why hello there world, you're a long string with some truncation!",
					"Why hello there world! You're a long string!"
				)
			);

			Assert.IsType<EqualException>(ex);
			Assert.Equal(
				"Assert.Equal() Failure: Strings differ" + Environment.NewLine +
				"                                  ↓ (pos 21)" + Environment.NewLine +
				$"Expected: {ArgumentFormatter.Ellipsis}\"hy hello there world, you're a long strin\"{ArgumentFormatter.Ellipsis}" + Environment.NewLine +
				$"Actual:   {ArgumentFormatter.Ellipsis}\"hy hello there world! You're a long strin\"{ArgumentFormatter.Ellipsis}" + Environment.NewLine +
				"                                  ↑ (pos 21)",
				ex.Message
			);
		}
	}

	public class Matches_Pattern
	{
		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("expectedRegexPattern", () => Assert.Matches((string?)null!, "Hello, world!"));
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
				"Assert.Matches() Failure: Pattern not found in value" + Environment.NewLine +
				@"Regex: ""\\d+""" + Environment.NewLine +
				@"Value: ""Hello, world!""",
				ex.Message
			);
		}

		[Fact]
		public void Failure_NullActual()
		{
			var ex = Record.Exception(() => Assert.Matches(@"\d+", null));

			Assert.IsType<MatchesException>(ex);
			Assert.Equal(
				"Assert.Matches() Failure: Pattern not found in value" + Environment.NewLine +
				@"Regex: ""\\d+""" + Environment.NewLine +
				"Value: null",
				ex.Message
			);
		}
	}

	public class Matches_Regex
	{
		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("expectedRegex", () => Assert.Matches((Regex?)null!, "Hello, world!"));
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
				"Assert.Matches() Failure: Pattern not found in value" + Environment.NewLine +
				@"Regex: ""\\d+""" + Environment.NewLine +
				@"Value: ""Hello, world!""",
				ex.Message
			);
		}

		[Fact]
		public void Failure_NullActual()
		{
			var ex = Record.Exception(() => Assert.Matches(new Regex(@"\d+"), null));

			Assert.IsType<MatchesException>(ex);
			Assert.Equal(
				"Assert.Matches() Failure: Pattern not found in value" + Environment.NewLine +
				@"Regex: ""\\d+""" + Environment.NewLine +
				"Value: null",
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
		public void Failure()
		{
			var ex = Record.Exception(() => Assert.StartsWith("hey", "Hello, world!"));

			Assert.IsType<StartsWithException>(ex);
			Assert.Equal(
				"Assert.StartsWith() Failure: String start does not match" + Environment.NewLine +
				"String:         \"Hello, world!\"" + Environment.NewLine +
				"Expected start: \"hey\"",
				ex.Message
			);
		}

		[Fact]
		public void CaseSensitiveByDefault()
		{
			var ex = Record.Exception(() => Assert.StartsWith("WORLD!", "world!"));

			Assert.IsType<StartsWithException>(ex);
			Assert.Equal(
				"Assert.StartsWith() Failure: String start does not match" + Environment.NewLine +
				"String:         \"world!\"" + Environment.NewLine +
				"Expected start: \"WORLD!\"",
				ex.Message
			);
		}

		[Fact]
		public void CanSpecifyComparisonType()
		{
			Assert.StartsWith("HELLO", "Hello, world!", StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void NullStrings()
		{
			var ex = Record.Exception(() => Assert.StartsWith(default(string), default(string)));

			Assert.IsType<StartsWithException>(ex);
			Assert.Equal(
				"Assert.StartsWith() Failure: String start does not match" + Environment.NewLine +
				"String:         null" + Environment.NewLine +
				"Expected start: null",
				ex.Message
			);
		}

		[Fact]
		public void Truncation()
		{
			var expected = "This is a long string that we're looking for at the start";
			var actual = "This is the long string that we expected to find this starting inside";

			var ex = Record.Exception(() => Assert.StartsWith(expected, actual));

			Assert.IsType<StartsWithException>(ex);
			Assert.Equal(
				"Assert.StartsWith() Failure: String start does not match" + Environment.NewLine +
				"String:         \"This is the long string that we expected \"" + ArgumentFormatter.Ellipsis + Environment.NewLine +
				"Expected start: \"This is a long string that we're looking \"" + ArgumentFormatter.Ellipsis,
				ex.Message
			);
		}
	}
}
