namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test has failed.
    /// </summary>
    public interface ITestFailed : ITestResultMessage, IExecutionMessage, IFailureInformation
    {
    }
}