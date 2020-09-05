using System;
using Xunit;
using Xunit.Sdk;

public class SpanAssertsTests
{
	public class Contains
	{
		[Fact]
		public void CanSearchForReadOnlySpanSubstrings()
		{
			Assert.Contains("wor".AsSpan(), "Hello, world!".AsSpan());
		}

		[Fact]
		public void CanSearchForSpanSubstrings()
		{
			Assert.Contains("wor".Spanify(), "Hello, world!".Spanify());
		}

		[Fact]
		public void SubstringReadOnlyContainsIsCaseSensitiveByDefault()
		{
			var ex = Record.Exception(() => Assert.Contains("WORLD".AsSpan(), "Hello, world!".AsSpan()));

			Assert.IsType<ContainsException>(ex);
			Assert.Equal(
				"Assert.Contains() Failure" + Environment.NewLine +
				"Not found: WORLD" + Environment.NewLine +
				"In value:  Hello, world!",
				ex.Message
			);
		}

		[Fact]
		public void SubstringSpanContainsIsCaseSensitiveByDefault()
		{
			var ex = Record.Exception(() => Assert.Contains("WORLD".Spanify(), "Hello, world!".Spanify()));

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
			Assert.Throws<ContainsException>(() => Assert.Contains("hey".AsSpan(), "Hello, world!".AsSpan()));
		}

		[Fact]
		public void SubstringSpanNotFound()
		{
			Assert.Throws<ContainsException>(() => Assert.Contains("hey".Spanify(), "Hello, world!".Spanify()));
		}

		[Fact]
		public void NullActualReadOnlyIntThrows()
		{
			Assert.Throws<ContainsException>(() => Assert.Contains("foo".AsSpan(), ((string?)null).AsSpan()));
		}

		[Fact]
		public void SuccessWithIntReadonly()
		{
			Assert.Contains(new int[] { 3, 4, }.RoSpanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoSpanify());
		}

		[Fact]
		public void SuccessWithStringReadonly()
		{
			Assert.Contains(new string[] { "test", "it", }.RoSpanify(), new string[] { "something", "interesting", "test", "it", "out", }.RoSpanify());
		}

		[Fact]
		public void SuccessWithIntSpan()
		{
			Assert.Contains(new int[] { 3, 4, }.Spanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Spanify());
		}

		[Fact]
		public void NotFoundWithIntReadonly()
		{
			Assert.Throws<ContainsException>(() => Assert.Contains(new int[] { 13, 14, }.RoSpanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoSpanify()));
		}

		[Fact]
		public void NotFoundWithNonStringSpan()
		{
			Assert.Throws<ContainsException>(() => Assert.Contains(new int[] { 13, 14, }.Spanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Spanify()));
		}
	}

	public class Contains_WithComparisonType
	{
		[Fact]
		public void CanSearchForReadonlySubstringsCaseInsensitive()
		{
			Assert.Contains("WORLD".AsSpan(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void CanSearchForSpanSubstringsCaseInsensitive()
		{
			Assert.Contains("WORLD".Spanify(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
		}
	}

	public class DoesNotContain
	{
		[Fact]
		public void CanSearchForReadonlySubstrings()
		{
			Assert.DoesNotContain("hey".AsSpan(), "Hello, world!".AsSpan());
		}

		[Fact]
		public void CanSearchForSpanSubstrings()
		{
			Assert.DoesNotContain("hey".Spanify(), "Hello, world!".Spanify());
		}

		[Fact]
		public void SubstringReadonlyDoesNotContainIsCaseSensitiveByDefault()
		{
			Assert.DoesNotContain("WORLD".AsSpan(), "Hello, world!".AsSpan());
		}

		[Fact]
		public void SubstringSpanDoesNotContainIsCaseSensitiveByDefault()
		{
			Assert.DoesNotContain("WORLD".Spanify(), "Hello, world!".Spanify());
		}

		[Fact]
		public void SubstringReadonlyFound()
		{
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain("world".AsSpan(), "Hello, world!".AsSpan()));
		}

		[Fact]
		public void SubstringSpanFound()
		{
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain("world".Spanify(), "Hello, world!".Spanify()));
		}

		[Fact]
		public void NullActualStringReadonlyDoesNotThrow()
		{
			Assert.DoesNotContain("foo".AsSpan(), ((string?)null).AsSpan());
		}

		[Fact]
		public void SuccessWithNonStringReadonly()
		{
			Assert.DoesNotContain(new int[] { 13, 14, }.RoSpanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoSpanify());
		}

		[Fact]
		public void SuccessWithNonStringSpan()
		{
			Assert.DoesNotContain(new int[] { 13, 14, }.Spanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Spanify());
		}

		[Fact]
		public void NotFoundWithNonStringReadonly()
		{
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain(new int[] { 3, 4, }.RoSpanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.RoSpanify()));
		}

		[Fact]
		public void NotFoundWithNonStringSpan()
		{
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain(new int[] { 3, 4, }.Spanify(), new int[] { 1, 2, 3, 4, 5, 6, 7 }.Spanify()));
		}
	}

	public class DoesNotContain_WithComparisonType
	{
		[Fact]
		public void CanSearchForSubstringsReadonlyCaseInsensitive()
		{
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain("WORLD".AsSpan(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase));
		}

		[Fact]
		public void CanSearchForSubstringsSpanCaseInsensitive()
		{
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain("WORLD".Spanify(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase));
		}
	}

	public class Equal
	{
		[Theory]
		// Null values
		[InlineData(null, null, false, false, false)]
		[InlineData(null, "", false, false, false)]//unlike string, ReadonlySpan<char> of null acts like an empty string, not a distinct value, thus this is equal
		[InlineData("", null, false, false, false)]
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
		public void SuccessReadonlyCases(string? value1, string? value2, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.AsSpan(), value2.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);
			Assert.Equal(value2.AsSpan(), value1.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);
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
		public void SuccessSpanCases(string value1, string value2, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.Spanify(), value2.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);
			Assert.Equal(value2.Spanify(), value1.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);
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
		public void FailureReadonlyCases(string? expected, string? actual, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected.AsSpan(), actual.AsSpan(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences)
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
		public void FailureSpanCases(string expected, string actual, bool ignoreCase, bool ignoreLineEndingDifferences, bool ignoreWhiteSpaceDifferences, int expectedIndex, int actualIndex)
		{
			var ex = Record.Exception(
				() => Assert.Equal(expected.Spanify(), actual.Spanify(), ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences)
			);

			var eqEx = Assert.IsType<EqualException>(ex);
			Assert.Equal(expectedIndex, eqEx.ExpectedIndex);
			Assert.Equal(actualIndex, eqEx.ActualIndex);
		}


		[Theory]
		// Null values
		[InlineData(null, null)]
		[InlineData(null, new int[] { })]//unlike string, ReadonlySpan<char> of null acts like an empty string, not a distinct value, thus this is equal
		[InlineData(new int[] { }, null)]
		// Identical values
		[InlineData(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 })]
		public void SuccessReadonlyCasesInt(int[]? value1, int[]? value2)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.RoSpanify(), value2.RoSpanify());
			Assert.Equal(value2.RoSpanify(), value1.RoSpanify());
		}

		[Theory]
		// Null values
		[InlineData(null, null)]
		[InlineData(null, new int[] { })]//unlike string, ReadonlySpan<char> of null acts like an empty string, not a distinct value, thus this is equal
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
		public void FailureReadonlyCasesInt(int[]? expected, int[]? actual, int expectedIndex, int actualIndex)
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
		[InlineData(null, new string[] { })]//unlike string, ReadonlySpan<char> of null acts like an empty string, not a distinct value, thus this is equal
		[InlineData(new string[] { }, null)]
		// Identical values
		[InlineData(new string[] { "yes", "no", "maybe" }, new string[] { "yes", "no", "maybe" })]
		public void SuccessReadonlyCasesString(String[]? value1, String[]? value2)
		{
			// Run them in both directions, as the values should be interchangeable when they're equal
			Assert.Equal(value1.RoSpanify(), value2.RoSpanify());
			Assert.Equal(value2.RoSpanify(), value1.RoSpanify());
		}

		// Null values
		[InlineData(null, null)]
		[InlineData(null, new string[] { })]//unlike string, ReadonlySpan<char> of null acts like an empty string, not a distinct value, thus this is equal
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
		public void FailureReadonlyCasesString(string[]? expected, string[]? actual, int expectedIndex, int actualIndex)
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
		public void FailureSpanCasesString(String[]? expected, String[]? actual, int expectedIndex, int actualIndex)
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
		public void SuccessReadonly()
		{
			Assert.StartsWith("Hello".AsSpan(), "Hello, world!".AsSpan());
		}

		[Fact]
		public void SuccessSpan()
		{
			Assert.StartsWith("Hello".Spanify(), "Hello, world!".Spanify());
		}


		[Fact]
		public void IsCaseSensitiveByDefaultReadonly()
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
		public void NotFoundReadonly()
		{
			Assert.Throws<StartsWithException>(() => Assert.StartsWith("hey".AsSpan(), "Hello, world!".AsSpan()));
		}


		[Fact]
		public void NotFoundSpan()
		{
			Assert.Throws<StartsWithException>(() => Assert.StartsWith("hey".Spanify(), "Hello, world!".Spanify()));
		}

		[Fact]
		public void NullActualStringThrowsReadonly()
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
		public void CanSearchForSubstringsCaseInsensitiveReadonly()
		{
			Assert.StartsWith("HELLO".AsSpan(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void CanSearchForSubstringsCaseInsensitiveSpan()
		{
			Assert.StartsWith("HELLO".Spanify(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
		}
	}

	public class EndsWith
	{
		[Fact]
		public void SuccessReadonly()
		{
			Assert.EndsWith("world!".AsSpan(), "Hello, world!".AsSpan());
		}

		[Fact]
		public void SuccessSpan()
		{
			Assert.EndsWith("world!".Spanify(), "Hello, world!".Spanify());
		}

		[Fact]
		public void IsCaseSensitiveByDefaultReadonly()
		{
			var ex = Record.Exception(() => Assert.EndsWith("WORLD!".AsSpan(), "world!".AsSpan()));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure:" + Environment.NewLine +
				"Expected: WORLD!" + Environment.NewLine +
				"Actual:   world!",
				ex.Message
			);
		}

		[Fact]
		public void IsCaseSensitiveByDefaultSpan()
		{
			var ex = Record.Exception(() => Assert.EndsWith("WORLD!".Spanify(), "world!".Spanify()));

			Assert.IsType<EndsWithException>(ex);
			Assert.Equal(
				"Assert.EndsWith() Failure:" + Environment.NewLine +
				"Expected: WORLD!" + Environment.NewLine +
				"Actual:   world!",
				ex.Message
			);
		}

		[Fact]
		public void NotFoundReadonly()
		{
			Assert.Throws<EndsWithException>(() => Assert.EndsWith("hey".AsSpan(), "Hello, world!".AsSpan()));
		}

		[Fact]
		public void NotFoundSpan()
		{
			Assert.Throws<EndsWithException>(() => Assert.EndsWith("hey".Spanify(), "Hello, world!".Spanify()));
		}

		[Fact]
		public void NullActualStringThrowsReadonly()
		{
			Assert.Throws<EndsWithException>(() => Assert.EndsWith("foo".AsSpan(), null));
		}

		[Fact]
		public void NullActualStringThrowsSpan()
		{
			Assert.Throws<EndsWithException>(() => Assert.EndsWith("foo".Spanify(), null));
		}
	}

	public class EndsWith_WithComparisonType
	{
		[Fact]
		public void CanSearchForSubstringsCaseInsensitiveReadonly()
		{
			Assert.EndsWith("WORLD!".AsSpan(), "Hello, world!".AsSpan(), StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void CanSearchForSubstringsCaseInsensitiveSpan()
		{
			Assert.EndsWith("WORLD!".Spanify(), "Hello, world!".Spanify(), StringComparison.OrdinalIgnoreCase);
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
