namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that an error has occurred during test case cleanup. 
    /// </summary>
    public interface ITestCaseCleanupFailure : ITestCaseMessage, IExecutionMessage, IFailureInformation
    {
    }
}