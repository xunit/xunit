using System;
using TestDriven.Framework;
using Xunit;
using Xunit.Runner.TdNet;
using Xunit.Sdk;

public class TestResultMessageExtensionsTests
{
    [Fact]
    public void ConvertsDataFromXunitToTdNetFormat()
    {
        var testCase = Mocks.TestCase<TestResultMessageExtensionsTests>("ConvertsDataFromXunitToTdNetFormat");
        var message = new TestResultMessage
        {
            TestCase = testCase,
            TestDisplayName = "Display Name",
            ExecutionTime = 123.45M
        };

        var result = message.ToTdNetTestResult(TestState.Ignored);

        Assert.Same(testCase.GetClass(), result.FixtureType);
        Assert.Same(testCase.GetMethod(), result.Method);
        Assert.Equal("Display Name", result.Name);
        Assert.Equal(TimeSpan.FromTicks(1234500), result.TimeSpan);
        Assert.Equal(1, result.TotalTests);
    }
}