using Xunit;
using Xunit.Sdk;

public class XunitExceptionTests
{
	[Fact]
	public void PreservesUserMessage()
	{
		var ex = new XunitException("UserMessage");

		Assert.Equal("UserMessage", ex.UserMessage);
	}

	[Fact]
	public void UserMessageIsTheMessage()
	{
		var ex = new XunitException("UserMessage");

		Assert.Equal(ex.UserMessage, ex.Message);
	}
}
