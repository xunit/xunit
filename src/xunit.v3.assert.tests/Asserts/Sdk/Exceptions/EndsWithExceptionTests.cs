using System;
using Xunit;

public class EndsWithExceptionTests
{
	[Fact]
	public static void ExpectedAndActualSameLength_NoTruncation()
	{
		var expectedMessage =
			"Assert.EndsWith() Failure:" + Environment.NewLine +
			"Expected: WORLD" + Environment.NewLine +
			"Actual:   Hello";

		var ex = Record.Exception(() => Assert.EndsWith("WORLD", "Hello"));

		Assert.NotNull(ex);
		Assert.Equal(expectedMessage, ex.Message);
	}

	[Fact]
	public static void ActualLongerThanExpected_TruncatesActual()
	{
		var expectedMessage =
			"Assert.EndsWith() Failure:" + Environment.NewLine +
			"Expected:    WORLD" + Environment.NewLine +
			"Actual:   ···world";

		var ex = Record.Exception(() => Assert.EndsWith("WORLD", "Hello, world"));

		Assert.NotNull(ex);
		Assert.Equal(expectedMessage, ex.Message);
	}

	[Fact]
	public static void ActualNull()
	{
		var expectedMessage =
			"Assert.EndsWith() Failure:" + Environment.NewLine +
			"Expected: first test 1" + Environment.NewLine +
			"Actual:   (null)";

		var ex = Record.Exception(() => Assert.EndsWith("first test 1", null));

		Assert.NotNull(ex);
		Assert.Equal(expectedMessage, ex.Message);
	}

	[Fact]
	public static void ExpectedNull()
	{
		var expectedMessage =
			"Assert.EndsWith() Failure:" + Environment.NewLine +
			"Expected: (null)" + Environment.NewLine +
			"Actual:   first test 1";

		var ex = Record.Exception(() => Assert.EndsWith(null, "first test 1"));

		Assert.NotNull(ex);
		Assert.Equal(expectedMessage, ex.Message);
	}
}
