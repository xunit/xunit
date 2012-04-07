using Xunit;

public class TestMethodTests
{
    [Fact]
    public void ConstructorWillCreateEmptyTraitListWhenPassedNull()
    {
        TestMethod method = new TestMethod("method1", "Display Name", traits: null);

        Assert.NotNull(method.Traits);
    }

    [Fact]
    public void MethodWithNoRunResultsIsInNotRunState()
    {
        TestMethod method = new TestMethod("methdName", "displayname", null);

        Assert.Equal(TestStatus.NotRun, method.RunStatus);
    }

    [Fact]
    public void MethodWithAllPassingResultsIsInPassedRunState()
    {
        TestMethod method = new TestMethod("methdName", "displayname", null);

        method.RunResults.Add(new TestPassedResult(0.0, "displayname", null));
        method.RunResults.Add(new TestPassedResult(1.2, "displayname", null));

        Assert.Equal(TestStatus.Passed, method.RunStatus);
    }

    [Fact]
    public void MethodWithMixedPassingAndSkippedResultsButNoFailedResultsIsInSkippedRunState()
    {
        TestMethod method = new TestMethod("methdName", "displayname", null);

        method.RunResults.Add(new TestPassedResult(0.0, "displayname", null));
        method.RunResults.Add(new TestSkippedResult("displayname", "reason"));
        method.RunResults.Add(new TestPassedResult(1.2, "displayname", null));

        Assert.Equal(TestStatus.Skipped, method.RunStatus);
    }

    [Fact]
    public void MethodWithMixedPassingAndSkippedAndFailedResultsIsInFailedRunState()
    {
        TestMethod method = new TestMethod("methdName", "displayname", null);

        method.RunResults.Add(new TestPassedResult(0.0, "displayname", null));
        method.RunResults.Add(new TestSkippedResult("displayname", "reason"));
        method.RunResults.Add(new TestFailedResult(2.3, "displayname", null, "exceptionType", "exceptionMessage", "stackTrace"));
        method.RunResults.Add(new TestSkippedResult("displayname", "reason2"));
        method.RunResults.Add(new TestPassedResult(1.2, "displayname", null));

        Assert.Equal(TestStatus.Failed, method.RunStatus);
    }
}
