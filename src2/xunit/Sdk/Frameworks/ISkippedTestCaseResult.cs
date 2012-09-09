namespace Xunit.Sdk
{
    public interface ISkippedTestCaseResult : ITestCaseResult
    {
        string Reason { get; }
    }
}
