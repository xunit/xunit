#if XUNIT_SPAN

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Sdk;

public class MemoryAssertsTests
{
	public class Contains
	{
		public class Strings
		{
			[Fact]
			public void ReadOnlyMemory_Success()
			{
				Assert.Contains("wor".AsMemory(), "Hello, world!".AsMemory());
			}

			[Fact]
			public void ReadWriteMemory_Success()
			{
				Assert.Contains("wor".Memoryify(), "Hello, world!".Memoryify());
			}

			[Fact]
			public void ReadOnlyMemory_CaseSensitiveByDefault()
			{
				var ex = Record.Exception(() => Assert.Contains("WORLD".AsMemory(), "Hello, world!".AsMemory()));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
					"String:    \"Hello, world!\"" + Environment.NewLine +
					"Not found: \"WORLD\"",
					ex.Message
				);
			}

			[Fact]
			public void ReadWriteMemory_CaseSensitiveByDefault()
			{
				var ex = Record.Exception(() => Assert.Contains("WORLD".Memoryify(), "Hello, world!".Memoryify()));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
					"String:    \"Hello, world!\"" + Environment.NewLine +
					"Not found: \"WORLD\"",
					ex.Message
				);
			}

			[Fact]
			public void ReadOnlyMemory_CanSpecifyComparisonType()
			{
				Assert.Contains("WORLD".AsMemory(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
			}

			[Fact]
			public void ReadWriteMemory_CanSpecifyComparisonType()
			{
				Assert.Contains("WORLD".Memoryify(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
			}

			[Fact]
			public void ReadOnlyMemory_NullStringIsEmpty()
			{
				var ex = Record.Exception(() => Assert.Contains("foo".AsMemory(), default(string).AsMemory()));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
					"String:    \"\"" + Environment.NewLine +
					"Not found: \"foo\"",
					ex.Message
				);
			}

			[Fact]
			public void ReadWriteMemory_NullStringIsEmpty()
			{
				var ex = Record.Exception(() => Assert.Contains("foo".Memoryify(), default(string).Memoryify()));

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
						"We are looking for something very long as well".Memoryify(),
						"This is a relatively long string so that we can see the truncation in action".Memoryify()
					)
				);

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-string not found" + Environment.NewLine +
					"String:    \"This is a relatively long string so that \"" + ArgumentFormatter.Ellipsis + Environment.NewLine +
					"Not found: \"We are looking for something very long as\"" + ArgumentFormatter.Ellipsis,
					ex.Message
				);
			}
		}

		public class NonStrings
		{
			[Fact]
			public void ReadOnlyMemoryOfInts_Success()
			{
				Assert.Contains(new int[] { 3, 4 }.AsMemory(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.AsMemory());
			}

			[Fact]
			public void ReadOnlyMemoryOfStrings_Success()
			{
				Assert.Contains(new string[] { "test", "it" }.AsMemory(), new string[] { "something", "interesting", "test", "it", "out" }.AsMemory());
			}

			[Fact]
			public void ReadWriteMemoryOfInts_Success()
			{
				Assert.Contains(new int[] { 3, 4 }.Memoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Memoryify());
			}

			[Fact]
			public void ReadWriteMemoryOfStrings_Success()
			{
				Assert.Contains(new string[] { "test", "it" }.Memoryify(), new string[] { "something", "interesting", "test", "it", "out" }.Memoryify());
			}

			[Fact]
			public void ReadOnlyMemoryOfInts_Failure()
			{
				var ex = Record.Exception(() => Assert.Contains(new int[] { 13, 14 }.AsMemory(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.AsMemory()));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-memory not found" + Environment.NewLine +
					"Memory:    [1, 2, 3, 4, 5, " + ArgumentFormatter.Ellipsis + "]" + Environment.NewLine +
					"Not found: [13, 14]",
					ex.Message
				);
			}

			[Fact]
			public void ReadWriteMemoryOfInts_Failure()
			{
				var ex = Record.Exception(() => Assert.Contains(new int[] { 13, 14 }.Memoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Memoryify()));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-memory not found" + Environment.NewLine +
					"Memory:    [1, 2, 3, 4, 5, " + ArgumentFormatter.Ellipsis + "]" + Environment.NewLine +
					"Not found: [13, 14]",
					ex.Message
				);
			}

			[Fact]
			public void FindingNonEmptyMemoryInsideEmptyMemoryFails()
			{
				var ex = Record.Exception(() => Assert.Contains(new int[] { 3, 4 }.Memoryify(), Memory<int>.Empty));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-memory not found" + Environment.NewLine +
					"Memory:    []" + Environment.NewLine +
					"Not found: [3, 4]",
					ex.Message
				);
			}

			[Fact]
			public void FindingEmptyMemoryInsideAnyMemorySucceeds()
			{
				Assert.Contains(Memory<int>.Empty, new int[] { 3, 4 }.Memoryify());
				Assert.Contains(Memory<int>.Empty, Memory<int>.Empty);
			}
		}
	}

	public class DoesNotContain
	{
		public class Strings
		{
			[Fact]
			public void ReadOnlyMemory_Success()
			{
				Assert.DoesNotContain("hey".AsMemory(), "Hello, world!".AsMemory());
			}

			[Fact]
			public void ReadWriteMemory_Success()
			{
				Assert.DoesNotContain("hey".Memoryify(), "Hello, world!".Memoryify());
			}

			[Fact]
			public void ReadOnlyMemory_CaseSensitiveByDefault()
			{
				Assert.DoesNotContain("WORLD".AsMemory(), "Hello, world!".AsMemory());
			}

			[Fact]
			public void ReadWriteMemory_CaseSensitiveByDefault()
			{
				Assert.DoesNotContain("WORLD".Memoryify(), "Hello, world!".Memoryify());
			}

			[Fact]
			public void ReadOnlyMemory_CanSpecifyComparisonType()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain("WORLD".AsMemory(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase));

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
			public void ReadWriteMemory_CanSpecifyComparisonType()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain("WORLD".Memoryify(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase));

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
			public void ReadOnlyMemory_Failure()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain("world".AsMemory(), "Hello, world!".AsMemory()));

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
			public void ReadWriteMemory_Failure()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain("world".Memoryify(), "Hello, world!".Memoryify()));

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
			public void ReadOnlyMemory_NullStringIsEmpty()
			{
				Assert.DoesNotContain("foo".AsMemory(), default(string).AsMemory());
			}

			[Fact]
			public void ReadWriteMemory_NullStringIsEmpty()
			{
				Assert.DoesNotContain("foo".Memoryify(), default(string).Memoryify());
			}

			[Fact]
			public void VeryLongString_FoundAtFront()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain("world".Memoryify(), "Hello, world from a very long string that will end up being truncated".Memoryify()));

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
					"                ↓ (pos 7)" + Environment.NewLine +
					"String: \"Hello, world from a very long string that\"" + ArgumentFormatter.Ellipsis + Environment.NewLine +
					"Found:  \"world\"",
					ex.Message
				);
			}

			[Fact]
			public void VeryLongString_FoundInMiddle()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain("world".Memoryify(), "This is a relatively long string that has 'Hello, world' placed in the middle so that we can dual trunaction".Memoryify()));

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
					"                                ↓ (pos 50)" + Environment.NewLine +
					"String: " + ArgumentFormatter.Ellipsis + "\"ng that has 'Hello, world' placed in the \"" + ArgumentFormatter.Ellipsis + Environment.NewLine +
					"Found:  \"world\"",
					ex.Message
				);
			}

			[Fact]
			public void VeryLongString_FoundAtEnd()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain("world".Memoryify(), "This is a relatively long string that will from the front truncated, just to say 'Hello, world'".Memoryify()));

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-string found" + Environment.NewLine +
					"                                               ↓ (pos 89)" + Environment.NewLine +
					"String: " + ArgumentFormatter.Ellipsis + "\"ont truncated, just to say 'Hello, world'\"" + Environment.NewLine +
					"Found:  \"world\"",
					ex.Message
				);
			}
		}

		public class NonStrings
		{
			[Fact]
			public void ReadOnlyMemoryOfInts_Success()
			{
				Assert.DoesNotContain(new int[] { 13, 14 }.AsMemory(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.AsMemory());
			}

			[Fact]
			public void ReadOnlyMemoryOfStrings_Success()
			{
				Assert.DoesNotContain(new string[] { "it", "test" }.AsMemory(), new string[] { "something", "interesting", "test", "it", "out" }.AsMemory());
			}

			[Fact]
			public void ReadWriteMemoryOfInts_Success()
			{
				Assert.DoesNotContain(new int[] { 13, 14 }.Memoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Memoryify());
			}

			[Fact]
			public void ReadWriteMemoryOfStrings_Success()
			{
				Assert.DoesNotContain(new string[] { "it", "test" }.Memoryify(), new string[] { "something", "interesting", "test", "it", "out" }.Memoryify());
			}

			[Fact]
			public void ReadOnlyMemoryOfInts_Failure()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain(new int[] { 3, 4 }.AsMemory(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.AsMemory()));

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-memory found" + Environment.NewLine +
					"               ↓ (pos 2)" + Environment.NewLine +
					"Memory: [1, 2, 3, 4, 5, " + ArgumentFormatter.Ellipsis + "]" + Environment.NewLine +
					"Found:  [3, 4]",
					ex.Message
				);
			}

			[Fact]
			public void ReadWriteMemoryOfInts_Failure()
			{
				var ex = Record.Exception(() => Assert.DoesNotContain(new int[] { 3, 4 }.Memoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Memoryify()));

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-memory found" + Environment.NewLine +
					"               ↓ (pos 2)" + Environment.NewLine +
					"Memory: [1, 2, 3, 4, 5, " + ArgumentFormatter.Ellipsis + "]" + Environment.NewLine +
					"Found:  [3, 4]",
					ex.Message
				);
			}

			[Fact]
			public void SearchingForNonEmptyMemoryInsideEmptyMemorySucceeds()
			{
				Assert.DoesNotContain(new int[] { 3, 4 }.Memoryify(), Memory<int>.Empty);
			}

			[Theory]
			[InlineData(new[] { 3, 4 })]
			[InlineData(new int[0])]
			public void SearchForEmptyMemoryInsideAnyMemoryFails(IEnumerable<int> data)
			{
				var ex = Record.Exception(() => Assert.DoesNotContain(Memory<int>.Empty, data.ToArray().Memoryify()));

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-memory found" + Environment.NewLine +
					(data.Any() ? "         ↓ (pos 0)" + Environment.NewLine : "") +
					"Memory: " + CollectionTracker<int>.FormatStart(data) + Environment.NewLine +
					"Found:  []",
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
			Assert.EndsWith("world!".AsMemory(), "Hello, world!".AsMemory());
			Assert.EndsWith("world!".AsMemory(), "Hello, world!".Memoryify());
			Assert.EndsWith("world!".Memoryify(), "Hello, world!".AsMemory());
			Assert.EndsWith("world!".Memoryify(), "Hello, world!".Memoryify());
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

			assertFailure(() => Assert.EndsWith("hey".AsMemory(), "Hello, world!".AsMemory()));
			assertFailure(() => Assert.EndsWith("hey".AsMemory(), "Hello, world!".Memoryify()));
			assertFailure(() => Assert.EndsWith("hey".Memoryify(), "Hello, world!".AsMemory()));
			assertFailure(() => Assert.EndsWith("hey".Memoryify(), "Hello, world!".Memoryify()));
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

			assertFailure(() => Assert.EndsWith("WORLD!".AsMemory(), "world!".AsMemory()));
			assertFailure(() => Assert.EndsWith("WORLD!".AsMemory(), "world!".Memoryify()));
			assertFailure(() => Assert.EndsWith("WORLD!".Memoryify(), "world!".AsMemory()));
			assertFailure(() => Assert.EndsWith("WORLD!".Memoryify(), "world!".Memoryify()));
		}

		[Fact]
		public void CanSpecifyComparisonType()
		{
			Assert.EndsWith("WORLD!".AsMemory(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
			Assert.EndsWith("WORLD!".AsMemory(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
			Assert.EndsWith("WORLD!".Memoryify(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
			Assert.EndsWith("WORLD!".Memoryify(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
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

			assertFailure(() => Assert.EndsWith("foo".AsMemory(), null));
			assertFailure(() => Assert.EndsWith("foo".Memoryify(), null));
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

			assertFailure(() => Assert.EndsWith(expected.AsMemory(), actual.AsMemory()));
			assertFailure(() => Assert.EndsWith(expected.AsMemory(), actual.Memoryify()));
			assertFailure(() => Assert.EndsWith(expected.Memoryify(), actual.AsMemory()));
			assertFailure(() => Assert.EndsWith(expected.Memoryify(), actual.Memoryify()));
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
				// Run everything in both directions, as the values should be interchangeable when they're equal

				// ReadOnlyMemory vs. ReadOnlyMemory
				Assert.Equal(value1.AsMemory(), value2.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
				Assert.Equal(value2.AsMemory(), value1.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);

				// ReadOnlyMemory vs. Memory
				Assert.Equal(value1.AsMemory(), value2.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
				Assert.Equal(value2.AsMemory(), value1.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);

				// Memory vs. ReadOnlyMemory
				Assert.Equal(value1.Memoryify(), value2.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
				Assert.Equal(value2.Memoryify(), value1.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);

				// Memory vs. Memory
				Assert.Equal(value1.Memoryify(), value2.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
				Assert.Equal(value2.Memoryify(), value1.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
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

				assertFailure(() => Assert.Equal(expected.AsMemory(), actual.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
				assertFailure(() => Assert.Equal(expected.Memoryify(), actual.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
				assertFailure(() => Assert.Equal(expected.AsMemory(), actual.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
				assertFailure(() => Assert.Equal(expected.Memoryify(), actual.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace));
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
						"Expected: " + ArgumentFormatter.Ellipsis + "\"hy hello there world, you're a long strin\"" + ArgumentFormatter.Ellipsis + Environment.NewLine +
						"Actual:   " + ArgumentFormatter.Ellipsis + "\"hy hello there world! You're a long strin\"" + ArgumentFormatter.Ellipsis + Environment.NewLine +
						"                                  ↑ (pos 21)",
						ex.Message
					);
				}

				assertFailure(
					() => Assert.Equal(
						"Why hello there world, you're a long string with some truncation!".AsMemory(),
						"Why hello there world! You're a long string!".AsMemory()
					)
				);
				assertFailure(
					() => Assert.Equal(
						"Why hello there world, you're a long string with some truncation!".AsMemory(),
						"Why hello there world! You're a long string!".Memoryify()
					)
				);
				assertFailure(
					() => Assert.Equal(
						"Why hello there world, you're a long string with some truncation!".Memoryify(),
						"Why hello there world! You're a long string!".AsMemory()
					)
				);
				assertFailure(
					() => Assert.Equal(
						"Why hello there world, you're a long string with some truncation!".Memoryify(),
						"Why hello there world! You're a long string!".Memoryify()
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
				Assert.Equal(value1.AsMemory(), value2.AsMemory());
				Assert.Equal(value2.AsMemory(), value1.AsMemory());

				// ReadOnlySpan vs. Span
				Assert.Equal(value1.AsMemory(), value2.Memoryify());
				Assert.Equal(value2.AsMemory(), value1.Memoryify());

				// Span vs. ReadOnlySpan
				Assert.Equal(value1.Memoryify(), value2.AsMemory());
				Assert.Equal(value2.Memoryify(), value1.AsMemory());

				// Span vs. Span
				Assert.Equal(value1.Memoryify(), value2.Memoryify());
				Assert.Equal(value2.Memoryify(), value1.Memoryify());
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

				assertFailure(() => Assert.Equal(new int[] { 1, 0, 2, 3 }.AsMemory(), new int[] { 1, 2, 3 }.AsMemory()));
				assertFailure(() => Assert.Equal(new int[] { 1, 0, 2, 3 }.AsMemory(), new int[] { 1, 2, 3 }.Memoryify()));
				assertFailure(() => Assert.Equal(new int[] { 1, 0, 2, 3 }.Memoryify(), new int[] { 1, 2, 3 }.AsMemory()));
				assertFailure(() => Assert.Equal(new int[] { 1, 0, 2, 3 }.Memoryify(), new int[] { 1, 2, 3 }.Memoryify()));
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

				assertFailure(() => Assert.Equal(new int[] { 1, 2, 3 }.AsMemory(), new int[] { 1, 2, 3, 4 }.AsMemory()));
				assertFailure(() => Assert.Equal(new int[] { 1, 2, 3 }.AsMemory(), new int[] { 1, 2, 3, 4 }.Memoryify()));
				assertFailure(() => Assert.Equal(new int[] { 1, 2, 3 }.Memoryify(), new int[] { 1, 2, 3, 4 }.AsMemory()));
				assertFailure(() => Assert.Equal(new int[] { 1, 2, 3 }.Memoryify(), new int[] { 1, 2, 3, 4 }.Memoryify()));
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
				Assert.Equal(value1.AsMemory(), value2.AsMemory());
				Assert.Equal(value2.AsMemory(), value1.AsMemory());

				// ReadOnlyMemory vs. Memory
				Assert.Equal(value1.AsMemory(), value2.Memoryify());
				Assert.Equal(value2.AsMemory(), value1.Memoryify());

				// Memory vs. ReadOnlyMemory
				Assert.Equal(value1.Memoryify(), value2.AsMemory());
				Assert.Equal(value2.Memoryify(), value1.AsMemory());

				// Memory vs. Memory
				Assert.Equal(value1.Memoryify(), value2.Memoryify());
				Assert.Equal(value2.Memoryify(), value1.Memoryify());
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

				assertFailure(() => Assert.Equal(new string[] { "yes", "no", "maybe" }.AsMemory(), new string[] { "yes", "no", "maybe", "so" }.AsMemory()));
				assertFailure(() => Assert.Equal(new string[] { "yes", "no", "maybe" }.AsMemory(), new string[] { "yes", "no", "maybe", "so" }.Memoryify()));
				assertFailure(() => Assert.Equal(new string[] { "yes", "no", "maybe" }.Memoryify(), new string[] { "yes", "no", "maybe", "so" }.AsMemory()));
				assertFailure(() => Assert.Equal(new string[] { "yes", "no", "maybe" }.Memoryify(), new string[] { "yes", "no", "maybe", "so" }.Memoryify()));
			}
		}
	}

	public class StartsWith
	{
		[Fact]
		public void Success()
		{
			Assert.StartsWith("Hello".AsMemory(), "Hello, world!".AsMemory());
			Assert.StartsWith("Hello".AsMemory(), "Hello, world!".Memoryify());
			Assert.StartsWith("Hello".Memoryify(), "Hello, world!".AsMemory());
			Assert.StartsWith("Hello".Memoryify(), "Hello, world!".Memoryify());
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

			assertFailure(() => Assert.StartsWith("hey".AsMemory(), "Hello, world!".AsMemory()));
			assertFailure(() => Assert.StartsWith("hey".AsMemory(), "Hello, world!".Memoryify()));
			assertFailure(() => Assert.StartsWith("hey".Memoryify(), "Hello, world!".AsMemory()));
			assertFailure(() => Assert.StartsWith("hey".Memoryify(), "Hello, world!".Memoryify()));
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

			assertFailure(() => Assert.StartsWith("WORLD!".AsMemory(), "world!".AsMemory()));
			assertFailure(() => Assert.StartsWith("WORLD!".AsMemory(), "world!".Memoryify()));
			assertFailure(() => Assert.StartsWith("WORLD!".Memoryify(), "world!".AsMemory()));
			assertFailure(() => Assert.StartsWith("WORLD!".Memoryify(), "world!".Memoryify()));
		}

		[Fact]
		public void CanSpecifyComparisonType()
		{
			Assert.StartsWith("HELLO".AsMemory(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("HELLO".AsMemory(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("HELLO".Memoryify(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith("HELLO".Memoryify(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
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

			assertFailure(() => Assert.StartsWith("foo".AsMemory(), null));
			assertFailure(() => Assert.StartsWith("foo".Memoryify(), null));
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

			assertFailure(() => Assert.StartsWith(expected.AsMemory(), actual.AsMemory()));
			assertFailure(() => Assert.StartsWith(expected.AsMemory(), actual.Memoryify()));
			assertFailure(() => Assert.StartsWith(expected.Memoryify(), actual.AsMemory()));
			assertFailure(() => Assert.StartsWith(expected.Memoryify(), actual.Memoryify()));
		}
	}
}

#endif
