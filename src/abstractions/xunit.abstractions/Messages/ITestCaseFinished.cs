namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test case has finished executing.
    /// </summary>
    public interface ITestCaseFinished : ITestCaseMessage, IExecutionMessage, IFinishedMessage
    {
    }
}