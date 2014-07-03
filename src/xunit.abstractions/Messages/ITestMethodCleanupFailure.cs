namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that an error has occurred during test method cleanup. 
    /// </summary>
    public interface ITestMethodCleanupFailure : ITestMethodMessage, IExecutionMessage, IFailureInformation
    {
    }
}