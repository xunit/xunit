using System;

using Xunit;

public class StartsWithExceptionTests
{
    [Fact]
    public void ActualStringNotLongerThanActualStringDoesNotTruncateActualString()
    {
        string expectedMessage =
            "Assert.StartsWith() Failure:" + Environment.NewLine +
            "Expected: WORLD" + Environment.NewLine +
            "Actual:   Hello";

        var ex = Record.Exception(() => Assert.StartsWith("WORLD", "Hello"));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void ActualStringLongerThanActualStringTruncatesActualString()
    {
        string expectedMessage =
            "Assert.StartsWith() Failure:" + Environment.NewLine +
            "Expected: WORLD" + Environment.NewLine +
            "Actual:   Hello...";

        var ex = Record.Exception(() => Assert.StartsWith("WORLD", "Hello, world!"));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void ActualStringIsNullAndExpectedIsNotShowsNullPlaceholderText()
    {
        string expectedMessage =
            "Assert.StartsWith() Failure:" + Environment.NewLine +
            "Expected: first test 1" + Environment.NewLine +
            "Actual:   (null)";

        var ex = Record.Exception(() => Assert.StartsWith("first test 1", null));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void ExpectedStringIsNullAndActualIsNotShowsNullPlaceholderText()
    {
        string expectedMessage =
            "Assert.StartsWith() Failure:" + Environment.NewLine +
            "Expected: (null)" + Environment.NewLine +
            "Actual:   first test 1";

        var ex = Record.Exception(() => Assert.StartsWith(null, "first test 1"));

        Assert.Equal(expectedMessage, ex.Message);
    }
}
