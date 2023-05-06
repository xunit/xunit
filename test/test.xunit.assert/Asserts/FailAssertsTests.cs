using Xunit;
using Xunit.Sdk;

public class FailAssertsTests
{
	[Fact]
	public void WithoutMessage()
	{
		var ex = Record.Exception(() => Assert.Fail());

		Assert.IsType<FailException>(ex);
		Assert.Equal("Assert.Fail() Failure", ex.Message);
	}

	[Fact]
	public void WithMessage()
	{
		var ex = Record.Exception(() => Assert.Fail("This is a user message"));

		Assert.IsType<FailException>(ex);
		Assert.Equal("This is a user message", ex.Message);
	}
}
