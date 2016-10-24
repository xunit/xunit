using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestOutputHelperTests
{
    private readonly ITestOutputHelper _output;

    public TestOutputHelperTests(ITestOutputHelper output)
    {
        _output = output;
    }

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
    public void WriteLine(string message, string expected)
    {
        _output.WriteLine(message);
        if (_output is TestOutputHelper)
        {
            Assert.Equal(expected + Environment.NewLine, ((TestOutputHelper)_output).Output);
        }
    }
}
