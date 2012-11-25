namespace Xunit.Abstractions
{
    public interface ISkippedTestCaseResult : ITestCaseResult
    {
        string Reason { get; }
    }
}
