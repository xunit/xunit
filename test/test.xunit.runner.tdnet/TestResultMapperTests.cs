using TestDriven.Framework;
using Xunit;
using Xunit.Runner.TdNet;

public class TestResultMapperTests
{
    [Fact]
    public void MapTests()
    {
        Assert.Equal(TestRunState.Success, TestResultMapper.Map(TestRunnerResult.Passed));
        Assert.Equal(TestRunState.Failure, TestResultMapper.Map(TestRunnerResult.Failed));
        Assert.Equal(TestRunState.NoTests, TestResultMapper.Map(TestRunnerResult.NoTests));
    }

    [Fact]
    public void MergeTests()
    {
        Assert.Equal(TestRunState.NoTests, TestResultMapper.Merge(TestRunState.NoTests, TestRunState.NoTests));
        Assert.Equal(TestRunState.Failure, TestResultMapper.Merge(TestRunState.NoTests, TestRunState.Failure));
        Assert.Equal(TestRunState.Success, TestResultMapper.Merge(TestRunState.NoTests, TestRunState.Success));
        Assert.Equal(TestRunState.Failure, TestResultMapper.Merge(TestRunState.Failure, TestRunState.NoTests));
        Assert.Equal(TestRunState.Failure, TestResultMapper.Merge(TestRunState.Failure, TestRunState.Failure));
        Assert.Equal(TestRunState.Failure, TestResultMapper.Merge(TestRunState.Failure, TestRunState.Success));
        Assert.Equal(TestRunState.Success, TestResultMapper.Merge(TestRunState.Success, TestRunState.NoTests));
        Assert.Equal(TestRunState.Failure, TestResultMapper.Merge(TestRunState.Success, TestRunState.Failure));
        Assert.Equal(TestRunState.Success, TestResultMapper.Merge(TestRunState.Success, TestRunState.Success));
    }
}