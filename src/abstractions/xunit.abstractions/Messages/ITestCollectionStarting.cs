namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test collection has is about to start executing.
    /// </summary>
    public interface ITestCollectionStarting : ITestCollectionMessage, IExecutionMessage
    {
    }
}