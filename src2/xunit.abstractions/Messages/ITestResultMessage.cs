namespace Xunit.Abstractions
{
    public interface ITestResultMessage : ITestMessage
    {
        decimal ExecutionTime { get; }
        ITestCase TestCase { get; }
        string TestDisplayName { get; }
    }
}
