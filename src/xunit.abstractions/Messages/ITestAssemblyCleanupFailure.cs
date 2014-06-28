namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that an error has occurred in test assembly cleanup. 
    /// </summary>
    public interface ITestAssemblyCleanupFailure : ITestAssemblyMessage, IExecutionMessage, IFailureInformation
    {
    }
}