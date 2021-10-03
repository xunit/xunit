using System;

using Xunit;

public class StartsWithExceptionTests
{
	[Fact]
	public void ActualStringNotLongerThanActualStringDoesNotTruncateActualString()
	{
		var expectedMessage =
			"Assert.StartsWith() Failure:" + Environment.NewLine +
			"Expected: WORLD" + Environment.NewLine +
			"Actual:   Hello";

		var ex = Record.Exception(() => Assert.StartsWith("WORLD", "Hello"));

		Assert.NotNull(ex);
		Assert.Equal(expectedMessage, ex.Message);
	}

	[Fact]
	public void ActualStringLongerThanActualStringTruncatesActualString()
	{
		var expectedMessage =
			"Assert.StartsWith() Failure:" + Environment.NewLine +
			"Expected: WORLD" + Environment.NewLine +
			"Actual:   Hello...";

		var ex = Record.Exception(() => Assert.StartsWith("WORLD", "Hello, world!"));

		Assert.NotNull(ex);
		Assert.Equal(expectedMessage, ex.Message);
	}

	[Fact]
	public void ActualStringIsNullAndExpectedIsNotShowsNullPlaceholderText()
	{
		var expectedMessage =
			"Assert.StartsWith() Failure:" + Environment.NewLine +
			"Expected: first test 1" + Environment.NewLine +
			"Actual:   (null)";

		var ex = Record.Exception(() => Assert.StartsWith("first test 1", null));

		Assert.NotNull(ex);
		Assert.Equal(expectedMessage, ex.Message);
	}

	[Fact]
	public void ExpectedStringIsNullAndActualIsNotShowsNullPlaceholderText()
	{
		var expectedMessage =
			"Assert.StartsWith() Failure:" + Environment.NewLine +
			"Expected: (null)" + Environment.NewLine +
			"Actual:   first test 1";

		var ex = Record.Exception(() => Assert.StartsWith(null, "first test 1"));

		Assert.NotNull(ex);
		Assert.Equal(expectedMessage, ex.Message);
	}
}
