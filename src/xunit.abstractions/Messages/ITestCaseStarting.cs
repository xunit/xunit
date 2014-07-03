namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test case is about to start executing.
    /// </summary>
    public interface ITestCaseStarting : ITestCaseMessage, IExecutionMessage
    {
    }
}