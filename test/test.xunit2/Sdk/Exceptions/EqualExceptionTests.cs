using System;
using Xunit;
using Xunit.Sdk;

public class EqualExceptionTests
{
    [Fact]
    public void OneStringAddsValueToEndOfTheOtherString()
    {
        string expectedMessage =
            "Assert.Equal() Failure" + Environment.NewLine +
            "                    ↓ (pos 10)" + Environment.NewLine +
            "Expected: first test 1" + Environment.NewLine +
            "Actual:   first test" + Environment.NewLine +
            "                    ↑ (pos 10)";

        var ex = Record.Exception(() => Assert.Equal("first test 1", "first test"));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void OneStringOneNullDoesNotShowDifferencePoint()
    {
        string expectedMessage =
            "Assert.Equal() Failure" + Environment.NewLine +
            "Expected: first test 1" + Environment.NewLine +
            "Actual:   (null)";

        var ex = Record.Exception(() => Assert.Equal("first test 1", null));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void StringsDifferInTheMiddle()
    {
        string expectedMessage =
            "Assert.Equal() Failure" + Environment.NewLine +
            "                ↓ (pos 6)" + Environment.NewLine +
            "Expected: first failure" + Environment.NewLine +
            "Actual:   first test" + Environment.NewLine +
            "                ↑ (pos 6)";

        var ex = Record.Exception(() => Assert.Equal("first failure", "first test"));

        Assert.Equal(expectedMessage, ex.Message);
    }
}
