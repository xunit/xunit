using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestOutputHelperTests
{
    public static IEnumerable<object[]> InvalidStrings_TestData()
    {
        // Valid
        yield return new object[] { " \r \n \t  ", " \r \n \t  " };
        yield return new object[] { "Hello World", "Hello World" };
        yield return new object[] { "\uD800\uDC00", "\uD800\uDC00" };

        // Invalid
        yield return new object[] { "\0", "\\0" };
        yield return new object[] { (char)1, "\\x01" };
        yield return new object[] { (char)31, "\\x1f" };
        yield return new object[] { "\uD800", "\\xd800" };
        yield return new object[] { "\uDC00", "\\xdc00" };
        yield return new object[] { "\uDC00\uD800", "\\xdc00\\xd800" };
    }

    [Theory]
    [MemberData(nameof(InvalidStrings_TestData))]
    public void WriteLine(string outputText, string expected)
    {
        var output = new TestOutputHelper();
        var messageBus = new SpyMessageBus();
        var testCase = Mocks.TestCase();
        var test = Mocks.Test(testCase, "Test Display Name");
        output.Initialize(messageBus, test);

        output.WriteLine(outputText);

        var message = Assert.Single(messageBus.Messages);
        var outputMessage = Assert.IsAssignableFrom<ITestOutput>(message);
        Assert.Equal(expected + Environment.NewLine, outputMessage.Output);
        Assert.Equal(expected + Environment.NewLine, output.Output);
    }
}
