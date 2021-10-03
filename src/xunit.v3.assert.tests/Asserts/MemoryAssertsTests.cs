#if XUNIT_SPAN

using System;
using Xunit;
using Xunit.Sdk;

public class MemoryAssertsTests
{
	public class Contains
	{
		[Fact]
		public void CanSearchForReadOnlyMemorySubstrings()
		{
			Assert.Contains("wor".AsMemory(), "Hello, world!".AsMemory());
		}

		[Fact]
		public void CanSearchForMemorySubstrings()
		{
			Assert.Contains("wor".Memoryify(), "Hello, world!".Memoryify());
		}

		[Fact]
		public void SubstringReadOnlyContainsIsCaseSensitiveByDefault()
		{
			var ex = Record.Exception(() => Assert.Contains("WORLD".AsMemory(), "Hello, world!".AsMemory()));

			Assert.IsType<ContainsException>(ex);
			Assert.Equal(
				"Assert.Contains() Failure" + Environment.NewLine +
				"Not found: WORLD" + Environment.NewLine +
				"In value:  Hello, world!",
				ex.Message
			);
		}

		[Fact]
		public void SubstringMemoryContainsIsCaseSensitiveByDefault()
		{
			var ex = Record.Exception(() => Assert.Contains("WORLD".Memoryify(), "Hello, world!".Memoryify()));

			Assert.IsType<ContainsException>(ex);
			Assert.Equal(
				"Assert.Contains() Failure" + Environment.NewLine +
				"Not found: WORLD" + Environment.NewLine +
				"In value:  Hello, world!",
				ex.Message
			);
		}

		[Fact]
		public void SubstringReadOnlyNotFound()
		{
			Assert.Throws<ContainsException>(() => Assert.Contains("hey".AsMemory(), "Hello, world!".AsMemory()));
		}

		[Fact]
		public void SubstringMemoryNotFound()
		{
			Assert.Throws<ContainsException>(() => Assert.Contains("hey".Memoryify(), "Hello, world!".Memoryify()));
		}

		[Fact]
		public void NullActualReadOnlyIntThrows()
		{
			Assert.Throws<ContainsException>(() => Assert.Contains("foo".AsMemory(), ((string?)null).AsMemory()));
		}

		[Fact]
		public void SuccessWithIntReadOnly()
		{
			Assert.Contains(new int[] { 3, 4, }.RoMemoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoMemoryify());
		}

		[Fact]
		public void SuccessWithStringReadOnly()
		{
			Assert.Contains(new string[] { "test", "it", }.RoMemoryify(), new string[] { "something", "interesting", "test", "it", "out", }.RoMemoryify());
		}

		[Fact]
		public void SuccessWithIntMemory()
		{
			Assert.Contains(new int[] { 3, 4, }.Memoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Memoryify());
		}

		[Fact]
		public void NotFoundWithIntReadOnly()
		{
			Assert.Throws<ContainsException>(() => Assert.Contains(new int[] { 13, 14, }.RoMemoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoMemoryify()));
		}

		[Fact]
		public void NotFoundWithNonStringMemory()
		{
			Assert.Throws<ContainsException>(() => Assert.Contains(new int[] { 13, 14, }.Memoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Memoryify()));
		}
	}

	public class Contains_WithComparisonType
	{
		[Fact]
		public void CanSearchForReadOnlySubstringsCaseInsensitive()
		{
			Assert.Contains("WORLD".AsMemory(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void CanSearchForMemorySubstringsCaseInsensitive()
		{
			Assert.Contains("WORLD".Memoryify(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
		}
	}

	public class DoesNotContain
	{
		[Fact]
		public void CanSearchForReadOnlySubstrings()
		{
			Assert.DoesNotContain("hey".AsMemory(), "Hello, world!".AsMemory());
		}

		[Fact]
		public void CanSearchForMemorySubstrings()
		{
			Assert.DoesNotContain("hey".Memoryify(), "Hello, world!".Memoryify());
		}

		[Fact]
		public void SubstringReadOnlyDoesNotContainIsCaseSensitiveByDefault()
		{
			Assert.DoesNotContain("WORLD".AsMemory(), "Hello, world!".AsMemory());
		}

		[Fact]
		public void SubstringMemoryDoesNotContainIsCaseSensitiveByDefault()
		{
			Assert.DoesNotContain("WORLD".Memoryify(), "Hello, world!".Memoryify());
		}

		[Fact]
		public void SubstringReadOnlyFound()
		{
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain("world".AsMemory(), "Hello, world!".AsMemory()));
		}

		[Fact]
		public void SubstringMemoryFound()
		{
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain("world".Memoryify(), "Hello, world!".Memoryify()));
		}

		[Fact]
		public void NullActualStringReadOnlyDoesNotThrow()
		{
			Assert.DoesNotContain("foo".AsMemory(), ((string?)null).AsMemory());
		}

		[Fact]
		public void SuccessWithNonStringReadOnly()
		{
			Assert.DoesNotContain(new int[] { 13, 14, }.RoMemoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoMemoryify());
		}

		[Fact]
		public void SuccessWithNonStringMemory()
		{
			Assert.DoesNotContain(new int[] { 13, 14, }.Memoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Memoryify());
		}

		[Fact]
		public void NotFoundWithNonStringReadOnly()
		{
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain(new int[] { 3, 4, }.RoMemoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoMemoryify()));
		}

		[Fact]
		public void NotFoundWithNonStringMemory()
		{
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain(new int[] { 3, 4, }.Memoryify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Memoryify()));
		}
	}

	public class DoesNotContain_WithComparisonType
	{
		[Fact]
		public void CanSearchForSubstringsReadOnlyCaseInsensitive()
		{
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain("WORLD".AsMemory(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase));
		}

		[Fact]
		public void CanSearchForSubstringsMemoryCaseInsensitive()
		{
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain("WORLD".Memoryify(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase));
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

	public class EndsWith
	{
		[Fact]
		public void SuccessReadOnly()
		{
			Assert.EndsWith("world!".AsMemory(), "Hello, world!".AsMemory());
		}

		[Fact]
		public void SuccessMemory()
		{
			Assert.EndsWith("world!".Memoryify(), "Hello, world!".Memoryify());
		}

		[Fact]
		public void IsCaseSensitiveByDefaultReadOnly()
		{
			var ex = Record.Exception(() => Assert.EndsWith("WORLD!".AsMemory(), "world!".AsMemory()));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure:" + Environment.NewLine +
				"Expected: WORLD!" + Environment.NewLine +
				"Actual:   world!",
				ex.Message
			);
		}

		[Fact]
		public void IsCaseSensitiveByDefaultMemory()
		{
			var ex = Record.Exception(() => Assert.EndsWith("WORLD!".Memoryify(), "world!".Memoryify()));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure:" + Environment.NewLine +
				"Expected: WORLD!" + Environment.NewLine +
				"Actual:   world!",
				ex.Message
			);
		}

		[Fact]
		public void NotFoundReadOnly()
		{
			Assert.Throws<EndsWithException>(() => Assert.EndsWith("hey".AsMemory(), "Hello, world!".AsMemory()));
		}

		[Fact]
		public void NotFoundMemory()
		{
			Assert.Throws<EndsWithException>(() => Assert.EndsWith("hey".Memoryify(), "Hello, world!".Memoryify()));
		}

		[Fact]
		public void NullActualStringThrowsReadOnly()
		{
			Assert.Throws<EndsWithException>(() => Assert.EndsWith("foo".AsMemory(), null));
		}

		[Fact]
		public void NullActualStringThrowsMemory()
		{
			Assert.Throws<EndsWithException>(() => Assert.EndsWith("foo".Memoryify(), null));
		}
	}

	public class EndsWith_WithComparisonType
	{
		[Fact]
		public void CanSearchForSubstringsCaseInsensitiveReadOnly()
		{
			Assert.EndsWith("WORLD!".AsMemory(), "Hello, world!".AsMemory(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void CanSearchForSubstringsCaseInsensitiveMemory()
		{
			Assert.EndsWith("WORLD!".Memoryify(), "Hello, world!".Memoryify(), StringComparison.OrdinalIgnoreCase);
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
