using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

public class AssertActualExpectedExceptionTests
{
    IEnumerable<object> MakeEnumeration(params object[] values)
    {
        foreach (var value in values)
            yield return value;
    }

    [Fact]
    public void NullValuesInArraysCreateCorrectExceptionMessage()
    {
        var expectedValue = new string?[] { null, "hello" };
        var actualValue = new string?[] { null, "world" };

        var expectedMessage =
            "Message" + Environment.NewLine +
            "Expected: String[] [null, \"hello\"]" + Environment.NewLine +
            "Actual:   String[] [null, \"world\"]";

        var ex = new AssertActualExpectedException(expectedValue, actualValue, "Message");

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void ExpectedAndActualAreUsedInMessage()
    {
        var expectedMessage =
            "Message" + Environment.NewLine +
            "Expected: 2" + Environment.NewLine +
            "Actual:   1";

        var ex = new AssertActualExpectedException(2, 1, "Message");

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void PreservesExpectedAndActual()
    {
        var ex = new AssertActualExpectedException(2, 1, null);

        Assert.Equal("1", ex.Actual);
        Assert.Equal("2", ex.Expected);
        Assert.Null(ex.UserMessage);
    }

    [Fact]
    public void SameVisibleValueDifferentTypes()
    {
        string expectedMessage =
            "Message" + Environment.NewLine +
            "Expected: 1 (System.String)" + Environment.NewLine +
            "Actual:   1 (System.Int32)";

        AssertActualExpectedException ex =
            new AssertActualExpectedException("1", 1, "Message");

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void DifferentVisibleValueDifferentTypes()
    {
        string expectedMessage =
            "Message" + Environment.NewLine +
            "Expected: 2" + Environment.NewLine +
            "Actual:   1";

        AssertActualExpectedException ex =
            new AssertActualExpectedException("2", 1, "Message");

        Assert.Equal(expectedMessage, ex.Message);
    }
}
