using Xunit;
using Xunit.Sdk;

public class NotNullTests
{
    [Fact]
    public void NotNull()
    {
        Assert.NotNull(new object());
    }

    [Fact]
    public void NotNullThrowsException()
    {
        Assert.Throws<NotNullException>(() => Assert.NotNull(null));
    }
}