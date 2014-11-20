namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a line of output was provided for a test.
    /// </summary>
    public interface ITestOutput : ITestMessage, IExecutionMessage
    {
        /// <summary>
        /// Gets the line of output.
        /// </summary>
        string Output { get; }
    }
}