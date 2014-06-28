namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that an error has occurred during test collection cleanup. 
    /// </summary>
    public interface ITestCollectionCleanupFailure : ITestCollectionMessage, IExecutionMessage, IFailureInformation
    {
    }
}