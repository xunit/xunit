using System;
using Xunit;

public class EqualExceptionTests
{
	[Fact]
	public void OneStringAddsValueToEndOfTheOtherString()
	{
		var expectedMessage =
			"Assert.Equal() Failure" + Environment.NewLine +
			"                    ↓ (pos 10)" + Environment.NewLine +
			"Expected: first test 1" + Environment.NewLine +
			"Actual:   first test" + Environment.NewLine +
			"                    ↑ (pos 10)";

		var ex = Record.Exception(() => Assert.Equal("first test 1", "first test"));

		Assert.NotNull(ex);
		Assert.Equal(expectedMessage, ex.Message);
	}

	[Fact]
	public void OneStringOneNullDoesNotShowDifferencePoint()
	{
		var expectedMessage =
			"Assert.Equal() Failure" + Environment.NewLine +
			"Expected: first test 1" + Environment.NewLine +
			"Actual:   (null)";

		var ex = Record.Exception(() => Assert.Equal("first test 1", null));

		Assert.NotNull(ex);
		Assert.Equal(expectedMessage, ex.Message);
	}

	[Fact]
	public void StringsDifferInTheMiddle()
	{
		var expectedMessage =
			"Assert.Equal() Failure" + Environment.NewLine +
			"                ↓ (pos 6)" + Environment.NewLine +
			"Expected: first failure" + Environment.NewLine +
			"Actual:   first test" + Environment.NewLine +
			"                ↑ (pos 6)";

		var ex = Record.Exception(() => Assert.Equal("first failure", "first test"));

		Assert.NotNull(ex);
		Assert.Equal(expectedMessage, ex.Message);
	}
}
