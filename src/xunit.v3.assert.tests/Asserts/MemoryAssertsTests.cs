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
					"String:    Hello, world!" + Environment.NewLine +
					"Not found: WORLD",
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
					"String:    Hello, world!" + Environment.NewLine +
					"Not found: WORLD",
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
					"String:    (empty string)" + Environment.NewLine +
					"Not found: foo",
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
						"We are looking for something very long as well".Memoryify(),
						"This is a relatively long string so that we can see the truncation in action".Memoryify()
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
			public void ReadOnlyMemoryOfInts_Success()
			{
				Assert.Contains(new int[] { 3, 4 }.RoMemoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoMemoryify());
			}

			[Fact]
			public void ReadOnlyMemoryOfStrings_Success()
			{
				Assert.Contains(new string[] { "test", "it" }.RoMemoryify(), new string[] { "something", "interesting", "test", "it", "out" }.RoMemoryify());
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
				var ex = Record.Exception(() => Assert.Contains(new int[] { 13, 14 }.RoMemoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoMemoryify()));

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Sub-memory not found" + Environment.NewLine +
					"Memory:    [1, 2, 3, 4, 5, ···]" + Environment.NewLine +
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
					"Memory:    [1, 2, 3, 4, 5, ···]" + Environment.NewLine +
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
					"               ↓ (pos 7)" + Environment.NewLine +
					"String: Hello, world!" + Environment.NewLine +
					"Found:  WORLD",
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
					"               ↓ (pos 7)" + Environment.NewLine +
					"String: Hello, world!" + Environment.NewLine +
					"Found:  WORLD",
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
					"               ↓ (pos 7)" + Environment.NewLine +
					"String: Hello, world!" + Environment.NewLine +
					"Found:  world",
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
					"               ↓ (pos 7)" + Environment.NewLine +
					"String: Hello, world!" + Environment.NewLine +
					"Found:  world",
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
					"               ↓ (pos 7)" + Environment.NewLine +
					"String: Hello, world from a very long string that···" + Environment.NewLine +
					"Found:  world",
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
					"                               ↓ (pos 50)" + Environment.NewLine +
					"String: ···ng that has 'Hello, world' placed in the ···" + Environment.NewLine +
					"Found:  world",
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
			public void ReadOnlyMemoryOfInts_Success()
			{
				Assert.DoesNotContain(new int[] { 13, 14 }.RoMemoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoMemoryify());
			}

			[Fact]
			public void ReadOnlyMemoryOfStrings_Success()
			{
				Assert.DoesNotContain(new string[] { "it", "test" }.RoMemoryify(), new string[] { "something", "interesting", "test", "it", "out" }.RoMemoryify());
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
				var ex = Record.Exception(() => Assert.DoesNotContain(new int[] { 3, 4 }.RoMemoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoMemoryify()));

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Sub-memory found" + Environment.NewLine +
					"               ↓ (pos 2)" + Environment.NewLine +
					"Memory: [1, 2, 3, 4, 5, ···]" + Environment.NewLine +
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
					"Memory: [1, 2, 3, 4, 5, ···]" + Environment.NewLine +
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
					"         ↓ (pos 0)" + Environment.NewLine +
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
		public void ReadOnlyMemory_Success()
		{
			Assert.EndsWith("world!".AsMemory(), "Hello, world!".AsMemory());
		}

		[Fact]
		public void ReadWriteMemory_Success()
		{
			Assert.EndsWith("world!".Memoryify(), "Hello, world!".Memoryify());
		}

		[Fact]
		public void ReadOnlyMemory_CaseSensitiveByDefault()
		{
			var ex = Record.Exception(() => Assert.EndsWith("WORLD!".AsMemory(), "world!".AsMemory()));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       world!" + Environment.NewLine +
				"Expected end: WORLD!",
				ex.Message
			);
		}

		[Fact]
		public void ReadWriteMemory_CaseSensitiveByDefault()
		{
			var ex = Record.Exception(() => Assert.EndsWith("WORLD!".Memoryify(), "world!".Memoryify()));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       world!" + Environment.NewLine +
				"Expected end: WORLD!",
				ex.Message
			);
		}

		[Fact]
		public void ReadOnlyMemory_CanSpecifyComparisonType()
		{
			Assert.EndsWith("WORLD!".AsMemory(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void ReadWriteMemory_CanSpecifyComparisonType()
		{
			Assert.EndsWith("WORLD!".Memoryify(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void ReadOnlyMemory_Failure()
		{
			var ex = Record.Exception(() => Assert.EndsWith("hey".AsMemory(), "Hello, world!".AsMemory()));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       Hello, world!" + Environment.NewLine +
				"Expected end: hey",
				ex.Message
			);
		}

		[Fact]
		public void ReadWriteMemory_Failure()
		{
			var ex = Record.Exception(() => Assert.EndsWith("hey".Memoryify(), "Hello, world!".Memoryify()));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       Hello, world!" + Environment.NewLine +
				"Expected end: hey",
				ex.Message
			);
		}

		[Fact]
		public void ReadOnlyMemory_NullStringIsEmpty()
		{
			var ex = Record.Exception(() => Assert.EndsWith("foo".AsMemory(), null));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure: String end does not match" + Environment.NewLine +
				"String:       (empty string)" + Environment.NewLine +
				"Expected end: foo",
				ex.Message
			);
		}

		[Fact]
		public void ReadWriteMemory_NullStringIsEmpty()
		{
			var ex = Record.Exception(() => Assert.EndsWith("foo".Memoryify(), null));

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
			var ex = Record.Exception(() => Assert.EndsWith("This is a long string that we're looking for at the end".Memoryify(), "This is the long string that we expected to find this ending inside".Memoryify()));

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
		[InlineData(null, null, false, false, false)]
		// Null ReadOnlyMemory<char> acts like an empty string
		[InlineData(null, "", false, false, false)]
		[InlineData("", null, false, false, false)]
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
		public void SuccessReadOnlyCases(string? value1, string? value2, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.AsMemory(), value2.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);
			Assert.Equal(value2.AsMemory(), value1.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);
		}

		[Theory]
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
		public void SuccessMemoryCases(string value1, string value2, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.Memoryify(), value2.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);
			Assert.Equal(value2.Memoryify(), value1.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);
		}

		[Theory]
		// Non-identical values
		[InlineData("foo", "foo!", false, false, false, 3, 3)]
		[InlineData("foo", "foo\0", false, false, false, 3, 3)]
		// Case differences
		[InlineData("foo bar", "foo   Bar", false, true, true, 4, 6)]
		// Line ending differences
		[InlineData("foo \nbar", "FoO  \rbar", true, false, true, 4, 5)]
		// Whitespace differences
		[InlineData("foo\n bar", "FoO\r\n  bar", true, true, false, 5, 6)]
		public void FailureReadOnlyCases(string? expected, string? actual, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected.AsMemory(), actual.AsMemory(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences)
			);

			var eqEx = Assert.IsType<EqualException>(ex);
			Assert.Equal(expectedIndex, eqEx.ExpectedIndex);
			Assert.Equal(actualIndex, eqEx.ActualIndex);
		}

		[Theory]
		// Non-identical values
		[InlineData("foo", "foo!", false, false, false, 3, 3)]
		[InlineData("foo", "foo\0", false, false, false, 3, 3)]
		// Case differences
		[InlineData("foo bar", "foo   Bar", false, true, true, 4, 6)]
		// Line ending differences
		[InlineData("foo \nbar", "FoO  \rbar", true, false, true, 4, 5)]
		// Whitespace differences
		[InlineData("foo\n bar", "FoO\r\n  bar", true, true, false, 5, 6)]
		public void FailureMemoryCases(string expected, string actual, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected.Memoryify(), actual.Memoryify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences)
			);

			var eqEx = Assert.IsType<EqualException>(ex);
			Assert.Equal(expectedIndex, eqEx.ExpectedIndex);
			Assert.Equal(actualIndex, eqEx.ActualIndex);
		}

		[Theory]
		// Null values
		[InlineData(null, null)]
		[InlineData(null, new int[] { })] // Null ReadOnlyMemory<int> acts like an empty array
		[InlineData(new int[] { }, null)]
		// Identical values
		[InlineData(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 })]
		public void SuccessReadOnlyCasesInt(int[]? value1, int[]? value2)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.RoMemoryify(), value2.RoMemoryify());
			Assert.Equal(value2.RoMemoryify(), value1.RoMemoryify());
		}

		[Theory]
		// Null values
		[InlineData(null, null)]
		[InlineData(null, new int[] { })] // Null Memory<int> acts like an empty array
		[InlineData(new int[] { }, null)]
		// Identical values
		[InlineData(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 })]
		public void SuccessMemoryCasesInt(int[]? value1, int[]? value2)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.Memoryify(), value2.Memoryify());
			Assert.Equal(value2.Memoryify(), value1.Memoryify());
		}

		[Theory]
		// Non-identical values
		[InlineData(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3, 4 }, 3, 3)]
		[InlineData(new int[] { 0, 1, 2, 3 }, new int[] { 1, 2, 3 }, 0, 0)]
		public void FailureReadOnlyCasesInt(int[]? expected, int[]? actual, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected.RoMemoryify(), actual.RoMemoryify())
			);

			var eqEx = Assert.IsType<EqualException>(ex);
			Assert.Equal(expectedIndex, eqEx.ExpectedIndex);
			Assert.Equal(actualIndex, eqEx.ActualIndex);
		}

		[Theory]
		// Non-identical values
		[InlineData(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3, 4 }, 3, 3)]
		[InlineData(new int[] { 0, 1, 2, 3 }, new int[] { 1, 2, 3 }, 0, 0)]
		public void FailureMemoryCasesInt(int[]? expected, int[]? actual, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected.Memoryify(), actual.Memoryify())
			);
			var eqEx = Assert.IsType<EqualException>(ex);
			Assert.Equal(expectedIndex, eqEx.ExpectedIndex);
			Assert.Equal(actualIndex, eqEx.ActualIndex);
		}

		[Theory]
		// Null values
		[InlineData(null, null)]
		[InlineData(null, new string[] { })] // Null ReadOnlyMemory<string> acts like an empty array
		[InlineData(new string[] { }, null)]
		// Identical values
		[InlineData(new string[] { "yes", "no", "maybe" }, new string[] { "yes", "no", "maybe" })]
		public void SuccessReadOnlyCasesString(string[]? value1, string[]? value2)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.RoMemoryify(), value2.RoMemoryify());
			Assert.Equal(value2.RoMemoryify(), value1.RoMemoryify());
		}

		// Null values
		[InlineData(null, null)]
		[InlineData(null, new string[] { })] // Null Memory<string> acts like an empty array
		[InlineData(new string[] { }, null)]
		// Identical values
		[InlineData(new string[] { "yes", "no", "maybe" }, new string[] { "yes", "no", "maybe" })]
		public void SuccessMemoryCasesString(string[]? value1, string[]? value2)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.Memoryify(), value2.Memoryify());
			Assert.Equal(value2.Memoryify(), value1.Memoryify());
		}

		[Theory]
		// Non-identical values
		[InlineData(new string[] { "yes", "no", "maybe" }, new string[] { "yes", "no", "maybe", "so" }, 3, 3)]
		[InlineData(new string[] { "so", "yes", "no", "maybe" }, new string[] { "yes", "no", "maybe", "so" }, 0, 0)]
		public void FailureReadOnlyCasesString(string[]? expected, string[]? actual, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected.RoMemoryify(), actual.RoMemoryify())
			);

			var eqEx = Assert.IsType<EqualException>(ex);
			Assert.Equal(expectedIndex, eqEx.ExpectedIndex);
			Assert.Equal(actualIndex, eqEx.ActualIndex);
		}

		[Theory]
		// Non-identical values
		[InlineData(new string[] { "yes", "no", "maybe" }, new string[] { "yes", "no", "maybe", "so" }, 3, 3)]
		[InlineData(new string[] { "so", "yes", "no", "maybe" }, new string[] { "yes", "no", "maybe", "so" }, 0, 0)]
		public void FailureMemoryCasesString(string[]? expected, string[]? actual, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected.Memoryify(), actual.Memoryify())
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
			Assert.StartsWith("Hello".AsMemory(), "Hello, world!".AsMemory());
		}

		[Fact]
		public void SuccessMemory()
		{
			Assert.StartsWith("Hello".Memoryify(), "Hello, world!".Memoryify());
		}


		[Fact]
		public void IsCaseSensitiveByDefaultReadOnly()
		{
			var ex = Record.Exception(() => Assert.StartsWith("HELLO".AsMemory(), "Hello".AsMemory()));

			Assert.IsType<StartsWithException>(ex);
			Assert.Equal(
				"Assert.StartsWith() Failure:" + Environment.NewLine +
				"Expected: HELLO" + Environment.NewLine +
				"Actual:   Hello",
				ex.Message
			);
		}

		[Fact]
		public void IsCaseSensitiveByDefaultMemory()
		{
			var ex = Record.Exception(() => Assert.StartsWith("HELLO".Memoryify(), "Hello".Memoryify()));

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
			Assert.Throws<StartsWithException>(() => Assert.StartsWith("hey".AsMemory(), "Hello, world!".AsMemory()));
		}


		[Fact]
		public void NotFoundMemory()
		{
			Assert.Throws<StartsWithException>(() => Assert.StartsWith("hey".Memoryify(), "Hello, world!".Memoryify()));
		}

		[Fact]
		public void NullActualStringThrowsReadOnly()
		{
			Assert.Throws<StartsWithException>(() => Assert.StartsWith("foo".AsMemory(), null));
		}

		[Fact]
		public void NullActualStringThrowsMemory()
		{
			Assert.Throws<StartsWithException>(() => Assert.StartsWith("foo".Memoryify(), null));
		}
	}

	public class StartsWith_WithComparisonType
	{
		[Fact]
		public void CanSearchForSubstringsCaseInsensitiveReadOnly()
		{
			Assert.StartsWith("HELLO".AsMemory(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void CanSearchForSubstringsCaseInsensitiveMemory()
		{
			Assert.StartsWith("HELLO".Memoryify(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
		}
	}
}

public static class MemoryTestHelpers
{
	public static Memory<T> Memoryify<T>(this T[]? values)
	{
		return new Memory<T>(values);
	}

	public static ReadOnlyMemory<T> RoMemoryify<T>(this T[]? values)
	{
		return new ReadOnlyMemory<T>(values);
	}

	public static Memory<char> Memoryify(this string? value)
	{
		return new Memory<char>((value ?? string.Empty).ToCharArray());
	}
}

#endif
