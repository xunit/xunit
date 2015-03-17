namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test has finished executing.
    /// </summary>
    public interface ITestFinished : ITestMessage, IExecutionMessage
    {
        /// <summary>
        /// Gets the time spent executing the test, in seconds.
        /// </summary>
        decimal ExecutionTime { get; }

        /// <summary>
        /// The captured output of the test.
        /// </summary>
        string Output { get; }

        // TODO: How do we differentiate a test (when a test case has multiple tests)?
        // Is it solely by the display name of the test?
    }
}