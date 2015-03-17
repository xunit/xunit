namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test class is about to begin running.
    /// </summary>
    public interface ITestClassStarting : ITestClassMessage, IExecutionMessage
    {
    }
}