#if XUNIT_SPAN

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Sdk;

public class SpanAssertsTests
{
	public class Contains
	{
		public class Strings
		{
			[Fact]
			public void ReadOnlySpan_Success()
			{
				Assert.Contains("wor".AsSpan(), "Hello, world!".AsSpan());
			}

			[Fact]
			public void ReadWriteSpan_Success()
			{
				Assert.Contains("wor".Spanify(), "Hello, world!".Spanify());
			}

			[Fact]
			public void ReadOnlySpan_CaseSensitiveByDefault()
			{
				var ex = Record.Exception(() => Assert.Contains("WORLD".AsSpan(), "Hello, world!".AsSpan()));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
					"String:    \"Hello, world!\"" + Environment.NewLine +
					"Not found: \"WORLD\"",
					ex.Message
				);
			}

			[Fact]
			public void ReadWriteSpan_CaseSensitiveByDefault()
			{
				var ex = Record.Exception(() => Assert.Contains("WORLD".Spanify(), "Hello, world!".Spanify()));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
					"String:    \"Hello, world!\"" + Environment.NewLine +
					"Not found: \"WORLD\"",
					ex.Message
				);
			}

			[Fact]
			public void ReadOnlySpan_CanSpecifyComparisonType()
			{
				Assert.Contains("WORLD".AsSpan(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
			}

			[Fact]
			public void ReadWriteSpan_CanSpecifyComparisonType()
			{
				Assert.Contains("WORLD".Spanify(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
			}

			[Fact]
			public void ReadOnlySpan_NullStringIsEmpty()
			{
				var ex = Record.Exception(() => Assert.Contains("foo".AsSpan(), default(string).AsSpan()));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
					"String:    \"\"" + Environment.NewLine +
					"Not found: \"foo\"",
					ex.Message
				);
			}

			[Fact]
			public void ReadWriteSpan_NullStringIsEmpty()
			{
				var ex = Record.Exception(() => Assert.Contains("foo".Spanify(), default(string).Spanify()));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
					"String:    \"\"" + Environment.NewLine +
					"Not found: \"foo\"",
					ex.Message
				);
			}

			[Fact]
			public void VeryLongStrings()
			{
				var ex = Record.Exception(
					() => Assert.Contains(
						"We are looking for something very long as well".Spanify(),
						"This is a relatively long string so that we can see the truncation in action".Spanify()
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

		public class NonStrings
		{
			[Fact]
			public void ReadOnlySpanOfInts_Success()
			{
				Assert.Contains(new int[] { 3, 4 }.AsSpan(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.AsSpan());
			}

			[Fact]
			public void ReadOnlySpanOfStrings_Success()
			{
				Assert.Contains(new string[] { "test", "it" }.AsSpan(), new string[] { "something", "interesting", "test", "it", "out" }.AsSpan());
			}

			[Fact]
			public void ReadWriteSpanOfInts_Success()
			{
				Assert.Contains(new int[] { 3, 4 }.Spanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Spanify());
			}

			[Fact]
			public void ReadWriteSpanOfStrings_Success()
			{
				Assert.Contains(new string[] { "test", "it" }.Spanify(), new string[] { "something", "interesting", "test", "it", "out" }.Spanify());
			}

			[Fact]
			public void ReadOnlySpanOfInts_Failure()
			{
				var ex = Record.Exception(() => Assert.Contains(new int[] { 13, 14 }.AsSpan(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.AsSpan()));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-span not found" + Environment.NewLine +
					$"Span:      [1, 2, 3, 4, 5, {ArgumentFormatter.Ellipsis}]" + Environment.NewLine +
					"Not found: [13, 14]",
					ex.Message
				);
			}

			[Fact]
			public void ReadWriteSpanOfInts_Failure()
			{
				var ex = Record.Exception(() => Assert.Contains(new int[] { 13, 14 }.Spanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Spanify()));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-span not found" + Environment.NewLine +
					$"Span:      [1, 2, 3, 4, 5, {ArgumentFormatter.Ellipsis}]" + Environment.NewLine +
					"Not found: [13, 14]",
					ex.Message
				);
			}

			[Fact]
			public void FindingNonEmptySpanInsideEmptySpanFails()
			{
				var ex = Record.Exception(() => Assert.Contains(new int[] { 3, 4 }.Spanify(), Span<int>.Empty));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-span not found" + Environment.NewLine +
					"Span:      []" + Environment.NewLine +
					"Not found: [3, 4]",
					ex.Message
				);
			}

			[Fact]
			public void FindingEmptySpanInsideAnySpanSucceeds()
			{
				Assert.Contains(Span<int>.Empty, new int[] { 3, 4 }.Spanify());
				Assert.Contains(Span<int>.Empty, Span<int>.Empty);
			}
		}
	}

	public class DoesNotContain
	{
		public class Strings
		{
			[Fact]
			public void ReadOnlySpan_Success()
			{
				Assert.DoesNotContain("hey".AsSpan(), "Hello, world!".AsSpan());
			}

			[Fact]
			public void ReadWriteSpan_Success()
			{
				Assert.DoesNotContain("hey".Spanify(), "Hello, world!".Spanify());
			}

			[Fact]
			public void ReadOnlySpan_CaseSensitiveByDefault()
			{
				Assert.DoesNotContain("WORLD".AsSpan(), "Hello, world!".AsSpan());
			}

			[Fact]
			public void ReadWriteSpan_CaseSensitiveByDefault()
			{
				Assert.DoesNotContain("WORLD".Spanify(), "Hello, world!".Spanify());
			}

			[Fact]
			public void ReadOnlySpan_CanSpecifyComparisonType()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain("WORLD".AsSpan(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase));

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
					"                ↓ (pos 7)" + Environment.NewLine +
					"String: \"Hello, world!\"" + Environment.NewLine +
					"Found:  \"WORLD\"",
					ex.Message
				);
			}

			[Fact]
			public void ReadWriteSpan_CanSpecifyComparisonType()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain("WORLD".Spanify(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase));

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
					"                ↓ (pos 7)" + Environment.NewLine +
					"String: \"Hello, world!\"" + Environment.NewLine +
					"Found:  \"WORLD\"",
					ex.Message
				);
			}

			[Fact]
			public void ReadOnlySpan_Failure()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain("world".AsSpan(), "Hello, world!".AsSpan()));

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
			public void ReadWriteSpan_Failure()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain("world".Spanify(), "Hello, world!".Spanify()));

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
			public void ReadOnlySpan_NullStringIsEmpty()
			{
				Assert.DoesNotContain("foo".AsSpan(), default(string).AsSpan());
			}

			[Fact]
			public void ReadWriteSpan_NullStringIsEmpty()
			{
				Assert.DoesNotContain("foo".Spanify(), default(string).AsSpan());
			}

			[Fact]
			public void VeryLongString_FoundAtFront()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain("world".AsSpan(), "Hello, world from a very long string that will end up being truncated".AsSpan()));

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
				var ex = Record.Exception(() => Assert.DoesNotContain("world".AsSpan(), "This is a relatively long string that has 'Hello, world' placed in the middle so that we can dual trunaction".AsSpan()));

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
				var ex = Record.Exception(() => Assert.DoesNotContain("world".AsSpan(), "This is a relatively long string that will from the front truncated, just to say 'Hello, world'".AsSpan()));

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

		public class NonStrings
		{
			[Fact]
			public void ReadOnlySpanOfInts_Success()
			{
				Assert.DoesNotContain(new int[] { 13, 14 }.AsSpan(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.AsSpan());
			}

			[Fact]
			public void ReadOnlySpanOfStrings_Success()
			{
				Assert.DoesNotContain(new string[] { "it", "test" }.AsSpan(), new string[] { "something", "interesting", "test", "it", "out" }.AsSpan());
			}

			[Fact]
			public void ReadWriteSpanOfInts_Success()
			{
				Assert.DoesNotContain(new int[] { 13, 14 }.Spanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Spanify());
			}

			[Fact]
			public void ReadWriteSpanOfStrings_Success()
			{
				Assert.DoesNotContain(new string[] { "it", "test" }.Spanify(), new string[] { "something", "interesting", "test", "it", "out" }.Spanify());
			}

			[Fact]
			public void ReadOnlySpanOfInts_Failure()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain(new int[] { 3, 4 }.AsSpan(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.AsSpan()));

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-span found" + Environment.NewLine +
					"              ↓ (pos 2)" + Environment.NewLine +
					$"Span:  [1, 2, 3, 4, 5, {ArgumentFormatter.Ellipsis}]" + Environment.NewLine +
					"Found: [3, 4]",
					ex.Message
				);
			}

			[Fact]
			public void ReadWriteSpanOfInts_Failure()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain(new int[] { 3, 4 }.Spanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Spanify()));

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-span found" + Environment.NewLine +
					"              ↓ (pos 2)" + Environment.NewLine +
					$"Span:  [1, 2, 3, 4, 5, {ArgumentFormatter.Ellipsis}]" + Environment.NewLine +
					"Found: [3, 4]",
					ex.Message
				);
			}

			[Fact]
			public void FindingNonEmptySpanInsideEmptySpanSucceeds()
			{
				Assert.DoesNotContain(new int[] { 3, 4 }.Spanify(), Span<int>.Empty);
			}

			[Theory]
			[InlineData(new[] { 3, 4 })]
			[InlineData(new int[0])]
			public void FindingEmptySpanInsideAnySpanFails(IEnumerable<int> data)
			{
				var ex = Record.Exception(() => Assert.DoesNotContain(Span<int>.Empty, data.ToArray().Spanify()));

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-span found" + Environment.NewLine +
					(data.Any() ? "        ↓ (pos 0)" + Environment.NewLine : "") +
					"Span:  " + CollectionTracker<int>.FormatStart(data) + Environment.NewLine +
					"Found: []",
					ex.Message
				);
			}
		}
	}

	public class EndsWith
	{
		[Fact]
		public void Success()
		{
			Assert.EndsWith("world!".AsSpan(), "Hello, world!".AsSpan());
			Assert.EndsWith("world!".AsSpan(), "Hello, world!".Spanify());
			Assert.EndsWith("world!".Spanify(), "Hello, world!".AsSpan());
			Assert.EndsWith("world!".Spanify(), "Hello, world!".Spanify());
		}

		[Fact]
		public void Failure()
		{
			void assertFailure(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<EndsWithException>(ex);
				Assert.Equal(
					"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
					"String:       \"Hello, world!\"" + Environment.NewLine +
					"Expected end: \"hey\"",
					ex.Message
				);
			}

			assertFailure(() => Assert.EndsWith("hey".AsSpan(), "Hello, world!".AsSpan()));
			assertFailure(() => Assert.EndsWith("hey".AsSpan(), "Hello, world!".Spanify()));
			assertFailure(() => Assert.EndsWith("hey".Spanify(), "Hello, world!".AsSpan()));
			assertFailure(() => Assert.EndsWith("hey".Spanify(), "Hello, world!".Spanify()));
		}

		[Fact]
		public void CaseSensitiveByDefault()
		{
			void assertFailure(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<EndsWithException>(ex);
				Assert.Equal(
					"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
					"String:       \"world!\"" + Environment.NewLine +
					"Expected end: \"WORLD!\"",
					ex.Message
				);
			}

			assertFailure(() => Assert.EndsWith("WORLD!".AsSpan(), "world!".AsSpan()));
			assertFailure(() => Assert.EndsWith("WORLD!".AsSpan(), "world!".Spanify()));
			assertFailure(() => Assert.EndsWith("WORLD!".Spanify(), "world!".AsSpan()));
			assertFailure(() => Assert.EndsWith("WORLD!".Spanify(), "world!".Spanify()));
		}

		[Fact]
		public void CanSpecifyComparisonType()
		{
			Assert.EndsWith("WORLD!".AsSpan(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
			Assert.EndsWith("WORLD!".AsSpan(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
			Assert.EndsWith("WORLD!".Spanify(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
			Assert.EndsWith("WORLD!".Spanify(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void NullStringIsEmpty()
		{
			void assertFailure(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<EndsWithException>(ex);
				Assert.Equal(
					"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
					"String:       \"\"" + Environment.NewLine +
					"Expected end: \"foo\"",
					ex.Message
				);
			}

			assertFailure(() => Assert.EndsWith("foo".AsSpan(), null));
			assertFailure(() => Assert.EndsWith("foo".Spanify(), null));
		}

		[Fact]
		public void Truncation()
		{
			var expected = "This is a long string that we're looking for at the end";
			var actual = "This is the long string that we expected to find this ending inside";

			void assertFailure(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<EndsWithException>(ex);
				Assert.Equal(
					"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
					"String:       " + ArgumentFormatter.Ellipsis + "\"at we expected to find this ending inside\"" + Environment.NewLine +
					"Expected end: \"This is a long string that we're looking \"" + ArgumentFormatter.Ellipsis,
					ex.Message
				);
			}

			assertFailure(() => Assert.EndsWith(expected.AsSpan(), actual.AsSpan()));
			assertFailure(() => Assert.EndsWith(expected.AsSpan(), actual.Spanify()));
			assertFailure(() => Assert.EndsWith(expected.Spanify(), actual.AsSpan()));
			assertFailure(() => Assert.EndsWith(expected.Spanify(), actual.Spanify()));
		}
	}

	public class Equal
	{
		public class Chars_TreatedLikeStrings
		{
			[Theory]
			// Null values
			[InlineData(null, null, false, false, false, false)]
			// Null ReadOnlySpan<char> acts like an empty string
			[InlineData(null, "", false, false, false, false)]
			[InlineData("", null, false, false, false, false)]
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

				// ReadOnlySpan vs. ReadOnlySpan
				Assert.Equal(value1.AsSpan(), value2.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
				Assert.Equal(value2.AsSpan(), value1.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);

				// ReadOnlySpan vs. Span
				Assert.Equal(value1.AsSpan(), value2.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
				Assert.Equal(value2.AsSpan(), value1.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);

				// Span vs. ReadOnlySpan
				Assert.Equal(value1.Spanify(), value2.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
				Assert.Equal(value2.Spanify(), value1.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);

				// Span vs. Span
				Assert.Equal(value1.Spanify(), value2.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
				Assert.Equal(value2.Spanify(), value1.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			}

			[Theory]
			// Non-identical values
			[InlineData("foo", "foo!", false, false, false, false, null, "   ↑ (pos 3)")]
			[InlineData("foo\0", "foo\0\0", false, false, false, false, null, "     ↑ (pos 4)")]
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
				string expected,
				string actual,
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

				void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						message,
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(expected.AsSpan(), actual.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
				assertFailure(() => Assert.Equal(expected.Spanify(), actual.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
				assertFailure(() => Assert.Equal(expected.AsSpan(), actual.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
				assertFailure(() => Assert.Equal(expected.Spanify(), actual.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
			}

			[Fact]
			public void Truncation()
			{
				void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

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

				assertFailure(
					() => Assert.Equal(
						"Why hello there world, you're a long string with some truncation!".AsSpan(),
						"Why hello there world! You're a long string!".AsSpan()
					)
				);
				assertFailure(
					() => Assert.Equal(
						"Why hello there world, you're a long string with some truncation!".AsSpan(),
						"Why hello there world! You're a long string!".Spanify()
					)
				);
				assertFailure(
					() => Assert.Equal(
						"Why hello there world, you're a long string with some truncation!".Spanify(),
						"Why hello there world! You're a long string!".AsSpan()
					)
				);
				assertFailure(
					() => Assert.Equal(
						"Why hello there world, you're a long string with some truncation!".Spanify(),
						"Why hello there world! You're a long string!".Spanify()
					)
				);
			}
		}

		public class Ints
		{
			[Theory]
			// Null values
			[InlineData(null, null)]
			[InlineData(null, new int[] { })] // Null ReadOnlySpan<int> acts like an empty array
			[InlineData(new int[] { }, null)]
			// Identical values
			[InlineData(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 })]
			public void Success(
				int[]? value1,
				int[]? value2)
			{
				// Run them in both directions, as the values should be interchangeable when they're equal

				// ReadOnlySpan vs. ReadOnlySpan
				Assert.Equal(value1.AsSpan(), value2.AsSpan());
				Assert.Equal(value2.AsSpan(), value1.AsSpan());

				// ReadOnlySpan vs. Span
				Assert.Equal(value1.AsSpan(), value2.Spanify());
				Assert.Equal(value2.AsSpan(), value1.Spanify());

				// Span vs. ReadOnlySpan
				Assert.Equal(value1.Spanify(), value2.AsSpan());
				Assert.Equal(value2.Spanify(), value1.AsSpan());

				// Span vs. Span
				Assert.Equal(value1.Spanify(), value2.Spanify());
				Assert.Equal(value2.Spanify(), value1.Spanify());
			}

			[Fact]
			public void Failure_MidCollection()
			{
				void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
						"              ↓ (pos 1)" + Environment.NewLine +
						"Expected: [1, 0, 2, 3]" + Environment.NewLine +
						"Actual:   [1, 2, 3]" + Environment.NewLine +
						"              ↑ (pos 1)",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(new int[] { 1, 0, 2, 3 }.AsSpan(), new int[] { 1, 2, 3 }.AsSpan()));
				assertFailure(() => Assert.Equal(new int[] { 1, 0, 2, 3 }.AsSpan(), new int[] { 1, 2, 3 }.Spanify()));
				assertFailure(() => Assert.Equal(new int[] { 1, 0, 2, 3 }.Spanify(), new int[] { 1, 2, 3 }.AsSpan()));
				assertFailure(() => Assert.Equal(new int[] { 1, 0, 2, 3 }.Spanify(), new int[] { 1, 2, 3 }.Spanify()));
			}

			[Fact]
			public void Failure_BeyondEnd()
			{
				void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
						"Expected: [1, 2, 3]" + Environment.NewLine +
						"Actual:   [1, 2, 3, 4]" + Environment.NewLine +
						"                    ↑ (pos 3)",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(new int[] { 1, 2, 3 }.AsSpan(), new int[] { 1, 2, 3, 4 }.AsSpan()));
				assertFailure(() => Assert.Equal(new int[] { 1, 2, 3 }.AsSpan(), new int[] { 1, 2, 3, 4 }.Spanify()));
				assertFailure(() => Assert.Equal(new int[] { 1, 2, 3 }.Spanify(), new int[] { 1, 2, 3, 4 }.AsSpan()));
				assertFailure(() => Assert.Equal(new int[] { 1, 2, 3 }.Spanify(), new int[] { 1, 2, 3, 4 }.Spanify()));
			}
		}

		public class Strings
		{
			[Theory]
			// Null values
			[InlineData(null, null)]
			[InlineData(null, new string[] { })] // Null ReadOnlyMemory<string> acts like an empty array
			[InlineData(new string[] { }, null)]
			// Identical values
			[InlineData(new string[] { "yes", "no", "maybe" }, new string[] { "yes", "no", "maybe" })]
			public void Success(
				string[]? value1,
				string[]? value2)
			{
				// Run them in both directions, as the values should be interchangeable when they're equal

				// ReadOnlyMemory vs. ReadOnlyMemory
				Assert.Equal(value1.AsSpan(), value2.AsSpan());
				Assert.Equal(value2.AsSpan(), value1.AsSpan());

				// ReadOnlyMemory vs. Memory
				Assert.Equal(value1.AsSpan(), value2.Spanify());
				Assert.Equal(value2.AsSpan(), value1.Spanify());

				// Memory vs. ReadOnlyMemory
				Assert.Equal(value1.Spanify(), value2.AsSpan());
				Assert.Equal(value2.Spanify(), value1.AsSpan());

				// Memory vs. Memory
				Assert.Equal(value1.Spanify(), value2.Spanify());
				Assert.Equal(value2.Spanify(), value1.Spanify());
			}

			[Fact]
			public void Failure()
			{
				void assertFailure(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
						"Expected: [\"yes\", \"no\", \"maybe\"]" + Environment.NewLine +
						"Actual:   [\"yes\", \"no\", \"maybe\", \"so\"]" + Environment.NewLine +
						"                                 ↑ (pos 3)",
						ex.Message
					);
				}

				assertFailure(() => Assert.Equal(new string[] { "yes", "no", "maybe" }.AsSpan(), new string[] { "yes", "no", "maybe", "so" }.AsSpan()));
				assertFailure(() => Assert.Equal(new string[] { "yes", "no", "maybe" }.AsSpan(), new string[] { "yes", "no", "maybe", "so" }.Spanify()));
				assertFailure(() => Assert.Equal(new string[] { "yes", "no", "maybe" }.Spanify(), new string[] { "yes", "no", "maybe", "so" }.AsSpan()));
				assertFailure(() => Assert.Equal(new string[] { "yes", "no", "maybe" }.Spanify(), new string[] { "yes", "no", "maybe", "so" }.Spanify()));
			}
		}
	}

	public class StartsWith
	{
		[Fact]
		public void Success()
		{
			Assert.StartsWith("Hello".AsSpan(), "Hello, world!".AsSpan());
			Assert.StartsWith("Hello".AsSpan(), "Hello, world!".Spanify());
			Assert.StartsWith("Hello".Spanify(), "Hello, world!".AsSpan());
			Assert.StartsWith("Hello".Spanify(), "Hello, world!".Spanify());
		}

		[Fact]
		public void Failure()
		{
			void assertFailure(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<StartsWithException>(ex);
				Assert.Equal(
					"Assert.StartsWith() Failure: String start does not match" + Environment.NewLine +
					"String:         \"Hello, world!\"" + Environment.NewLine +
					"Expected start: \"hey\"",
					ex.Message
				);
			}

			assertFailure(() => Assert.StartsWith("hey".AsSpan(), "Hello, world!".AsSpan()));
			assertFailure(() => Assert.StartsWith("hey".AsSpan(), "Hello, world!".Spanify()));
			assertFailure(() => Assert.StartsWith("hey".Spanify(), "Hello, world!".AsSpan()));
			assertFailure(() => Assert.StartsWith("hey".Spanify(), "Hello, world!".Spanify()));
		}

		[Fact]
		public void CaseSensitiveByDefault()
		{
			void assertFailure(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<StartsWithException>(ex);
				Assert.Equal(
					"Assert.StartsWith() Failure: String start does not match" + Environment.NewLine +
					"String:         \"world!\"" + Environment.NewLine +
					"Expected start: \"WORLD!\"",
					ex.Message
				);
			}

			assertFailure(() => Assert.StartsWith("WORLD!".AsSpan(), "world!".AsSpan()));
			assertFailure(() => Assert.StartsWith("WORLD!".AsSpan(), "world!".Spanify()));
			assertFailure(() => Assert.StartsWith("WORLD!".Spanify(), "world!".AsSpan()));
			assertFailure(() => Assert.StartsWith("WORLD!".Spanify(), "world!".Spanify()));
		}

		[Fact]
		public void CanSpecifyComparisonType()
		{
			Assert.StartsWith("HELLO".AsSpan(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("HELLO".AsSpan(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("HELLO".Spanify(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("HELLO".Spanify(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void NullStringIsEmpty()
		{
			void assertFailure(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<StartsWithException>(ex);
				Assert.Equal(
					"Assert.StartsWith() Failure: String start does not match" + Environment.NewLine +
					"String:         \"\"" + Environment.NewLine +
					"Expected start: \"foo\"",
					ex.Message
				);
			}

			assertFailure(() => Assert.StartsWith("foo".AsSpan(), null));
			assertFailure(() => Assert.StartsWith("foo".Spanify(), null));
		}

		[Fact]
		public void Truncation()
		{
			var expected = "This is a long string that we're looking for at the start";
			var actual = "This is the long string that we expected to find this starting inside";

			void assertFailure(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<StartsWithException>(ex);
				Assert.Equal(
					"Assert.StartsWith() Failure: String start does not match" + Environment.NewLine +
					"String:         \"This is the long string that we expected \"" + ArgumentFormatter.Ellipsis + Environment.NewLine +
					"Expected start: \"This is a long string that we're looking \"" + ArgumentFormatter.Ellipsis,
					ex.Message
				);
			}

			assertFailure(() => Assert.StartsWith(expected.AsSpan(), actual.AsSpan()));
			assertFailure(() => Assert.StartsWith(expected.AsSpan(), actual.Spanify()));
			assertFailure(() => Assert.StartsWith(expected.Spanify(), actual.AsSpan()));
			assertFailure(() => Assert.StartsWith(expected.Spanify(), actual.Spanify()));
		}
	}
}

#endif
