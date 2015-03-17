namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that an error has occurred during test cleanup. 
    /// </summary>
    public interface ITestCleanupFailure : ITestMessage, IExecutionMessage, IFailureInformation
    {
    }
}