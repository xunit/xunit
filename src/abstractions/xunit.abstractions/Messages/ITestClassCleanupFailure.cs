namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that an error has occurred during test class cleanup. 
    /// </summary>
    public interface ITestClassCleanupFailure : ITestClassMessage, IExecutionMessage, IFailureInformation
    {
    }
}