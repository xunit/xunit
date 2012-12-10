namespace Xunit.Abstractions
{
    public interface ITestFinished : ITestMessage
    {
        decimal ExecutionTime { get; }
        ITestCase TestCase { get; }
        string TestDisplayName { get; }
    }
}
