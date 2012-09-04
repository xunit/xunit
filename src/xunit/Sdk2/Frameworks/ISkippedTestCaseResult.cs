namespace Xunit.Sdk2
{
    public interface ISkippedTestCaseResult : ITestCaseResult
    {
        string Reason { get; }
    }
}
