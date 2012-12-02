namespace Xunit.Abstractions
{
    public interface ITestResultMessage : ITestMessage
    {
        string DisplayName { get; }
        decimal ExecutionTime { get; }
        ITestCase TestCase { get; }
    }
}
