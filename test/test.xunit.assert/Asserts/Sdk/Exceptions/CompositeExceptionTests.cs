using System;
using Xunit;
using Xunit.Sdk;

public class CompositeExceptionTests
{
    [Fact]
    public void ReturnsAMessageForEachFailure()
    {
        var errors = new[]
        {
            new Tuple<int, Exception>(1, new Exception("Error 1")),
            new Tuple<int, Exception>(3, new Exception("Error 2")),
            new Tuple<int, Exception>(5, new Exception("Error 3")),
        };

        var ex = new CompositeException(5, errors);

        var expectedMessage = "3 out of 5 items in the collection did not pass." + Environment.NewLine +
                              Environment.NewLine +
                              "Assert.All() Failure" + Environment.NewLine +
                              "Error during validation of item at index 1" + Environment.NewLine +
                              "Inner exception: System.Exception: Error 1" + Environment.NewLine +
                              "=================================================================" + Environment.NewLine +
                              "Assert.All() Failure" + Environment.NewLine +
                              "Error during validation of item at index 3" + Environment.NewLine +
                              "Inner exception: System.Exception: Error 2" + Environment.NewLine +
                              "=================================================================" + Environment.NewLine +
                              "Assert.All() Failure" + Environment.NewLine +
                              "Error during validation of item at index 5" + Environment.NewLine +
                              "Inner exception: System.Exception: Error 3" + Environment.NewLine;
        Assert.Equal(expectedMessage, ex.Message);
    }
}