using System;
using Xunit;
using Xunit.Sdk;

public class AllExceptionTests
{
    [Fact]
    public static void ReturnsAMessageForEachFailure()
    {
        var errors = new[]
        {
            new Tuple<int, Exception>(1, new Exception($"Multi-line{Environment.NewLine}message")),
            new Tuple<int, Exception>(3, new Exception("Error 2")),
            new Tuple<int, Exception>(5, new Exception("Error 3")),
        };

        var ex = new AllException(5, errors);

        var expectedMessage = "Assert.All() Failure: 3 out of 5 items in the collection did not pass." + Environment.NewLine +
                              "[1]: System.Exception: Multi-line" + Environment.NewLine +
                              "     message" + Environment.NewLine +
                              "[3]: System.Exception: Error 2" + Environment.NewLine +
                              "[5]: System.Exception: Error 3";

        Assert.Equal(expectedMessage, ex.Message);
    }
}