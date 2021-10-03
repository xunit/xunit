using System;
using Xunit;
using Xunit.Sdk;

public class FailAssertsTests
{
	[Fact]
	public void GuardClause()
	{
		Assert.Throws<ArgumentNullException>("message", () => Assert.Fail(null!));
	}

	[Fact]
	public void ThrowsFailException()
	{
		var ex = Record.Exception(() => Assert.Fail("This is a user message"));

		Assert.IsType<FailException>(ex);
		Assert.Equal("Assert.Fail(): This is a user message", ex.Message);
	}
}
