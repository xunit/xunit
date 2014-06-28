namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test is about to start executing.
    /// </summary>
    public interface ITestStarting : ITestMessage, IExecutionMessage
    {
        // TODO: How do we differentiate a test (when a test case has multiple tests)?
        // Is it solely by the display name of the test?
    }
}