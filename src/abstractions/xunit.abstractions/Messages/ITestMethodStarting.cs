namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test method is about to begin executing.
    /// </summary>
    public interface ITestMethodStarting : ITestMethodMessage, IExecutionMessage
    {
    }
}