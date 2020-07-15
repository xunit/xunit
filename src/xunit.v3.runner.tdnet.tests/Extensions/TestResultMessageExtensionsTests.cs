using System;
using TestDriven.Framework;
using Xunit;
using Xunit.Runner.TdNet;

public class TestResultMessageExtensionsTests
{
    [Fact]
    public void ConvertsDataFromXunitToTdNetFormat()
    {
        var message = Mocks.TestResult<TestResultMessageExtensionsTests>("ConvertsDataFromXunitToTdNetFormat", "Display Name", 123.45M);

        var result = message.ToTdNetTestResult(TestState.Ignored, 42);

        Assert.Same(message.TestCase.GetClass(), result.FixtureType);
        Assert.Same(message.TestCase.GetMethod(), result.Method);
        Assert.Equal("Display Name", result.Name);
        Assert.Equal(TimeSpan.FromTicks(1234500), result.TimeSpan);
        Assert.Equal(42, result.TotalTests);
    }
}
