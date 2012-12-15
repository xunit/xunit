using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

public class AssertActualExpectedExceptionTests
{
    [Fact(Skip = "Currently disabled, would like a better implementation to match the string implementation")]
    public void ArraysShowDifferencePoint()
    {
        int[] actualValue = new int[] { 1, 2, 3, 4, 5 };
        int[] expectedValue = new int[] { 1, 2, 5, 7, 9 };

        string expectedMessage =
            "Message" + Environment.NewLine +
            "Position: First difference is at position 2" + Environment.NewLine +
            "Expected: Int32[] { 1, 2, 5, 7, 9 }" + Environment.NewLine +
            "Actual:   Int32[] { 1, 2, 3, 4, 5 }";

        AssertActualExpectedException ex =
            new AssertActualExpectedException(expectedValue, actualValue, "Message");

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact(Skip = "Currently disabled, would like a better implementation to match the string implementation")]
    public void ListsShowDifferencePoint()
    {
        var actualValue = new List<int> { 1, 2, 3, 4, 5 };
        var expectedValue = new List<int> { 1, 2, 5, 7, 9 };

        string expectedMessage =
            "Message" + Environment.NewLine +
            "Position: First difference is at position 2" + Environment.NewLine +
            "Expected: List<Int32> { 1, 2, 5, 7, 9 }" + Environment.NewLine +
            "Actual:   List<Int32> { 1, 2, 3, 4, 5 }";

        AssertActualExpectedException ex =
            new AssertActualExpectedException(expectedValue, actualValue, "Message");

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact(Skip = "Currently disabled, would like a better implementation to match the string implementation")]
    public void MixedEnumerationShowDifferencePoint()
    {
        var expectedValue = MakeEnumeration(1, 42, "Hello");
        var actualValue = MakeEnumeration(1, 2.3, "Goodbye");

        string expectedMessage =
            "Message" + Environment.NewLine +
            "Position: First difference is at position 1" + Environment.NewLine +
            "Expected: <MakeEnumeration>d__2 { 1, 42, \"Hello\" }" + Environment.NewLine +
            "Actual:   <MakeEnumeration>d__2 { 1, 2.3, \"Goodbye\" }";

        AssertActualExpectedException ex =
            new AssertActualExpectedException(expectedValue, actualValue, "Message");

        Assert.Equal(expectedMessage, ex.Message);
    }

    IEnumerable<object> MakeEnumeration(params object[] values)
    {
        foreach (var value in values)
            yield return value;
    }

    [Fact]
    public void NullValuesInArraysCreateCorrectExceptionMessage()
    {
        string[] expectedValue = new string[] { null, "hello" };
        string[] actualValue = new string[] { null, "world" };

        string expectedMessage =
            "Message" + Environment.NewLine +
            "Expected: String[] { (null), \"hello\" }" + Environment.NewLine +
            "Actual:   String[] { (null), \"world\" }";

        AssertActualExpectedException ex =
            new AssertActualExpectedException(expectedValue, actualValue, "Message");

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void ExpectedAndActualAreUsedInMessage()
    {
        string expectedMessage =
            "Message" + Environment.NewLine +
            "Expected: 2" + Environment.NewLine +
            "Actual:   1";

        AssertActualExpectedException ex =
            new AssertActualExpectedException(2, 1, "Message");

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void PreservesExpectedAndActual()
    {
        AssertActualExpectedException ex =
            new AssertActualExpectedException(2, 1, null);

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