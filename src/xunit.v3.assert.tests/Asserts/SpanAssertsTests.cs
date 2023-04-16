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
					"String:    Hello, world!" + Environment.NewLine +
					"Not found: WORLD",
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
					"String:    Hello, world!" + Environment.NewLine +
					"Not found: WORLD",
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
					"String:    (empty string)" + Environment.NewLine +
					"Not found: foo",
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
					"String:    (empty string)" + Environment.NewLine +
					"Not found: foo",
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
					"String:    This is a relatively long string so that ···" + Environment.NewLine +
					"Not found: We are looking for something very long as···",
					ex.Message
				);
			}
		}

		public class NonStrings
		{
			[Fact]
			public void ReadOnlySpanOfInts_Success()
			{
				Assert.Contains(new int[] { 3, 4 }.RoSpanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoSpanify());
			}

			[Fact]
			public void ReadOnlySpanOfStrings_Success()
			{
				Assert.Contains(new string[] { "test", "it" }.RoSpanify(), new string[] { "something", "interesting", "test", "it", "out" }.RoSpanify());
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
				var ex = Record.Exception(() => Assert.Contains(new int[] { 13, 14 }.RoSpanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoSpanify()));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-span not found" + Environment.NewLine +
					"Span:      [1, 2, 3, 4, 5, ···]" + Environment.NewLine +
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
					"Span:      [1, 2, 3, 4, 5, ···]" + Environment.NewLine +
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
					"               ↓ (pos 7)" + Environment.NewLine +
					"String: Hello, world!" + Environment.NewLine +
					"Found:  WORLD",
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
					"               ↓ (pos 7)" + Environment.NewLine +
					"String: Hello, world!" + Environment.NewLine +
					"Found:  WORLD",
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
					"               ↓ (pos 7)" + Environment.NewLine +
					"String: Hello, world!" + Environment.NewLine +
					"Found:  world",
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
					"               ↓ (pos 7)" + Environment.NewLine +
					"String: Hello, world!" + Environment.NewLine +
					"Found:  world",
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
					"               ↓ (pos 7)" + Environment.NewLine +
					"String: Hello, world from a very long string that···" + Environment.NewLine +
					"Found:  world",
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
					"                               ↓ (pos 50)" + Environment.NewLine +
					"String: ···ng that has 'Hello, world' placed in the ···" + Environment.NewLine +
					"Found:  world",
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
					"                                              ↓ (pos 89)" + Environment.NewLine +
					"String: ···ont truncated, just to say 'Hello, world'" + Environment.NewLine +
					"Found:  world",
					ex.Message
				);
			}
		}

		public class NonStrings
		{
			[Fact]
			public void ReadOnlySpanOfInts_Success()
			{
				Assert.DoesNotContain(new int[] { 13, 14 }.RoSpanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoSpanify());
			}

			[Fact]
			public void ReadOnlySpanOfStrings_Success()
			{
				Assert.DoesNotContain(new string[] { "it", "test" }.RoSpanify(), new string[] { "something", "interesting", "test", "it", "out" }.RoSpanify());
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
				var ex = Record.Exception(() => Assert.DoesNotContain(new int[] { 3, 4 }.RoSpanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoSpanify()));

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-span found" + Environment.NewLine +
					"              ↓ (pos 2)" + Environment.NewLine +
					"Span:  [1, 2, 3, 4, 5, ···]" + Environment.NewLine +
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
					"Span:  [1, 2, 3, 4, 5, ···]" + Environment.NewLine +
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
		public void ReadOnlySpan_Success()
		{
			Assert.EndsWith("world!".AsSpan(), "Hello, world!".AsSpan());
		}

		[Fact]
		public void ReadWriteSpan_Success()
		{
			Assert.EndsWith("world!".Spanify(), "Hello, world!".Spanify());
		}

		[Fact]
		public void ReadOnlySpan_CaseSensitiveByDefault()
		{
			var ex = Record.Exception(() => Assert.EndsWith("WORLD!".AsSpan(), "world!".AsSpan()));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       world!" + Environment.NewLine +
				"Expected end: WORLD!",
				ex.Message
			);
		}

		[Fact]
		public void ReadWriteSpan_CaseSensitiveByDefault()
		{
			var ex = Record.Exception(() => Assert.EndsWith("WORLD!".Spanify(), "world!".Spanify()));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       world!" + Environment.NewLine +
				"Expected end: WORLD!",
				ex.Message
			);
		}

		[Fact]
		public void ReadOnlySpan_CanSpecifyComparisonType()
		{
			Assert.EndsWith("WORLD!".AsSpan(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void ReadWriteSpan_CanSpecifyComparisonType()
		{
			Assert.EndsWith("WORLD!".Spanify(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void ReadOnlySpan_Failure()
		{
			var ex = Record.Exception(() => Assert.EndsWith("hey".AsSpan(), "Hello, world!".AsSpan()));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       Hello, world!" + Environment.NewLine +
				"Expected end: hey",
				ex.Message
			);
		}

		[Fact]
		public void ReadWriteSpan_Failure()
		{
			var ex = Record.Exception(() => Assert.EndsWith("hey".Spanify(), "Hello, world!".Spanify()));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       Hello, world!" + Environment.NewLine +
				"Expected end: hey",
				ex.Message
			);
		}

		[Fact]
		public void ReadOnlySpan_NullStringIsEmpty()
		{
			var ex = Record.Exception(() => Assert.EndsWith("foo".AsSpan(), null));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       (empty string)" + Environment.NewLine +
				"Expected end: foo",
				ex.Message
			);
		}

		[Fact]
		public void ReadWriteSpan_NullStringIsEmpty()
		{
			var ex = Record.Exception(() => Assert.EndsWith("foo".Spanify(), null));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       (empty string)" + Environment.NewLine +
				"Expected end: foo",
				ex.Message
			);
		}

		[Fact]
		public void LongStrings()
		{
			var ex = Record.Exception(() => Assert.EndsWith("This is a long string that we're looking for at the end".Spanify(), "This is the long string that we expected to find this ending inside".Spanify()));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       ···at we expected to find this ending inside" + Environment.NewLine +
				"Expected end: This is a long string that we're looking ···",
				ex.Message
			);
		}
	}

	public class Equal
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
		// All whitespace differences
		[InlineData("", "  ", false, false, false, true)]
		[InlineData("", "  ", false, false, true, true)]
		[InlineData("", "\t", false, false, true, true)]
		[InlineData("foobar", "foo bar", false, false, true, true)]
		public void SuccessReadOnlyCases(string? value1, string? value2, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences, bool ignoreAllWhiteSpace)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.AsSpan(), value2.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value2.AsSpan(), value1.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
		}

		[Theory]
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
		// All whitespace differences
		[InlineData("", "  ", false, false, false, true)]
		[InlineData("", "  ", false, false, true, true)]
		[InlineData("", "\t", false, false, true, true)]
		[InlineData("foobar", "foo bar", false, false, true, true)]
		public void SuccessSpanCases(string value1, string value2, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences, bool ignoreAllWhiteSpace)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.Spanify(), value2.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
			Assert.Equal(value2.Spanify(), value1.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace);
		}

		[Theory]
		// Non-identical values
		[InlineData("foo", "foo!", false, false, false, false, 3, 3)]
		[InlineData("foo", "foo\0", false, false, false, false, 3, 3)]
		// Case differences
		[InlineData("foo bar", "foo   Bar", false, true, true, false, 4, 6)]
		// Line ending differences
		[InlineData("foo \nbar", "FoO  \rbar", true, false, true, false, 4, 5)]
		// Whitespace differences
		[InlineData("foo\n bar", "FoO\r\n  bar", true, true, false, false, 5, 6)]
		public void FailureReadOnlyCases(string? expected, string? actual, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences, bool ignoreAllWhiteSpace, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected.AsSpan(), actual.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace)
			);

			var eqEx = Assert.IsType<EqualException>(ex);
			Assert.Equal(expectedIndex, eqEx.ExpectedIndex);
			Assert.Equal(actualIndex, eqEx.ActualIndex);
		}

		[Theory]
		// Non-identical values
		[InlineData("foo", "foo!", false, false, false, false, 3, 3)]
		[InlineData("foo", "foo\0", false, false, false, false, 3, 3)]
		// Case differences
		[InlineData("foo bar", "foo   Bar", false, true, true, false, 4, 6)]
		// Line ending differences
		[InlineData("foo \nbar", "FoO  \rbar", true, false, true, false, 4, 5)]
		// Whitespace differences
		[InlineData("foo\n bar", "FoO\r\n  bar", true, true, false, false, 5, 6)]
		public void FailureSpanCases(string expected, string actual, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences, bool ignoreAllWhiteSpace, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected.Spanify(), actual.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences, ignoreAllWhiteSpace)
			);

			var eqEx = Assert.IsType<EqualException>(ex);
			Assert.Equal(expectedIndex, eqEx.ExpectedIndex);
			Assert.Equal(actualIndex, eqEx.ActualIndex);
		}

		[Fact]
		public void StringMessageFormatting()
		{
			var ex = Record.Exception(() =>
				Assert.Equal(
					"Why hello there world, you're a long string with some truncation!".Spanify(),
					"Why hello there world! You're a long string!".Spanify()
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

		[Theory]
		// Null values
		[InlineData(null, null)]
		[InlineData(null, new int[] { })] // Null ReadOnlySpan<int> acts like an empty array
		[InlineData(new int[] { }, null)]
		// Identical values
		[InlineData(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 })]
		public void SuccessReadOnlyCasesInt(int[]? value1, int[]? value2)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.RoSpanify(), value2.RoSpanify());
			Assert.Equal(value2.RoSpanify(), value1.RoSpanify());
		}

		[Theory]
		// Null values
		[InlineData(null, null)]
		[InlineData(null, new int[] { })] // Null Span<int> acts like an empty array
		[InlineData(new int[] { }, null)]
		// Identical values
		[InlineData(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 })]
		public void SuccessSpanCasesInt(int[]? value1, int[]? value2)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.Spanify(), value2.Spanify());
			Assert.Equal(value2.Spanify(), value1.Spanify());
		}

		[Theory]
		// Non-identical values
		[InlineData(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3, 4 }, 3, 3)]
		[InlineData(new int[] { 0, 1, 2, 3 }, new int[] { 1, 2, 3 }, 0, 0)]
		public void FailureReadOnlyCasesInt(int[]? expected, int[]? actual, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected.RoSpanify(), actual.RoSpanify())
			);

			var eqEx = Assert.IsType<EqualException>(ex);
			Assert.Equal(expectedIndex, eqEx.ExpectedIndex);
			Assert.Equal(actualIndex, eqEx.ActualIndex);
		}

		[Theory]
		// Non-identical values
		[InlineData(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3, 4 }, 3, 3)]
		[InlineData(new int[] { 0, 1, 2, 3 }, new int[] { 1, 2, 3 }, 0, 0)]
		public void FailureSpanCasesInt(int[]? expected, int[]? actual, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected.Spanify(), actual.Spanify())
			);
			var eqEx = Assert.IsType<EqualException>(ex);
			Assert.Equal(expectedIndex, eqEx.ExpectedIndex);
			Assert.Equal(actualIndex, eqEx.ActualIndex);
		}

		[Theory]
		// Null values
		[InlineData(null, null)]
		[InlineData(null, new string[] { })] // Null ReadOnlySpan<string> acts like an empty array
		[InlineData(new string[] { }, null)]
		// Identical values
		[InlineData(new string[] { "yes", "no", "maybe" }, new string[] { "yes", "no", "maybe" })]
		public void SuccessReadOnlyCasesString(string[]? value1, string[]? value2)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.RoSpanify(), value2.RoSpanify());
			Assert.Equal(value2.RoSpanify(), value1.RoSpanify());
		}

		// Null values
		[InlineData(null, null)]
		[InlineData(null, new string[] { })] // Null Span<string> acts like an empty array
		[InlineData(new string[] { }, null)]
		// Identical values
		[InlineData(new string[] { "yes", "no", "maybe" }, new string[] { "yes", "no", "maybe" })]
		public void SuccessSpanCasesString(string[]? value1, string[]? value2)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.Spanify(), value2.Spanify());
			Assert.Equal(value2.Spanify(), value1.Spanify());
		}

		[Theory]
		// Non-identical values
		[InlineData(new string[] { "yes", "no", "maybe" }, new string[] { "yes", "no", "maybe", "so" }, 3, 3)]
		[InlineData(new string[] { "so", "yes", "no", "maybe" }, new string[] { "yes", "no", "maybe", "so" }, 0, 0)]
		public void FailureReadOnlyCasesString(string[]? expected, string[]? actual, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected.RoSpanify(), actual.RoSpanify())
			);

			var eqEx = Assert.IsType<EqualException>(ex);
			Assert.Equal(expectedIndex, eqEx.ExpectedIndex);
			Assert.Equal(actualIndex, eqEx.ActualIndex);
		}

		[Theory]
		// Non-identical values
		[InlineData(new string[] { "yes", "no", "maybe" }, new string[] { "yes", "no", "maybe", "so" }, 3, 3)]
		[InlineData(new string[] { "so", "yes", "no", "maybe" }, new string[] { "yes", "no", "maybe", "so" }, 0, 0)]
		public void FailureSpanCasesString(string[]? expected, string[]? actual, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected.Spanify(), actual.Spanify())
			);

			var eqEx = Assert.IsType<EqualException>(ex);
			Assert.Equal(expectedIndex, eqEx.ExpectedIndex);
			Assert.Equal(actualIndex, eqEx.ActualIndex);
		}
	}

	public class StartsWith
	{
		[Fact]
		public void SuccessReadOnly()
		{
			Assert.StartsWith("Hello".AsSpan(), "Hello, world!".AsSpan());
		}

		[Fact]
		public void SuccessSpan()
		{
			Assert.StartsWith("Hello".Spanify(), "Hello, world!".Spanify());
		}

		[Fact]
		public void IsCaseSensitiveByDefaultReadOnly()
		{
			var ex = Record.Exception(() => Assert.StartsWith("HELLO".AsSpan(), "Hello".AsSpan()));

			Assert.IsType<StartsWithException>(ex);
			Assert.Equal(
				"Assert.StartsWith() Failure:" + Environment.NewLine +
				"Expected: HELLO" + Environment.NewLine +
				"Actual:   Hello",
				ex.Message
			);
		}

		[Fact]
		public void IsCaseSensitiveByDefaultSpan()
		{
			var ex = Record.Exception(() => Assert.StartsWith("HELLO".Spanify(), "Hello".Spanify()));

			Assert.IsType<StartsWithException>(ex);
			Assert.Equal(
				"Assert.StartsWith() Failure:" + Environment.NewLine +
				"Expected: HELLO" + Environment.NewLine +
				"Actual:   Hello",
				ex.Message
			);
		}

		[Fact]
		public void NotFoundReadOnly()
		{
			Assert.Throws<StartsWithException>(() => Assert.StartsWith("hey".AsSpan(), "Hello, world!".AsSpan()));
		}


		[Fact]
		public void NotFoundSpan()
		{
			Assert.Throws<StartsWithException>(() => Assert.StartsWith("hey".Spanify(), "Hello, world!".Spanify()));
		}

		[Fact]
		public void NullActualStringThrowsReadOnly()
		{
			Assert.Throws<StartsWithException>(() => Assert.StartsWith("foo".AsSpan(), null));
		}

		[Fact]
		public void NullActualStringThrowsSpan()
		{
			Assert.Throws<StartsWithException>(() => Assert.StartsWith("foo".Spanify(), null));
		}
	}

	public class StartsWith_WithComparisonType
	{
		[Fact]
		public void CanSearchForSubstringsCaseInsensitiveReadOnly()
		{
			Assert.StartsWith("HELLO".AsSpan(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void CanSearchForSubstringsCaseInsensitiveSpan()
		{
			Assert.StartsWith("HELLO".Spanify(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
		}
	}
}

public static class SpanTestHelpers
{
	public static Span<T> Spanify<T>(this T[]? values)
	{
		return new Span<T>(values);
	}

	public static ReadOnlySpan<T> RoSpanify<T>(this T[]? values)
	{
		return new ReadOnlySpan<T>(values);
	}

	public static Span<char> Spanify(this string? value)
	{
		return new Span<char>((value ?? string.Empty).ToCharArray());
	}
}

#endif
