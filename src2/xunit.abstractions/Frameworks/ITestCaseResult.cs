namespace Xunit.Abstractions
{
    public interface ITestCaseResult
    {
        double ExecutionTime { get; }

        string Output { get; }

        ITestCase TestCase { get; }
    }
}
