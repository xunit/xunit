namespace Xunit.Abstractions
{
    /// <summary>
    /// This is the base interface for all individual test results (e.g., tests which
    /// pass, fail, or are skipped).
    /// </summary>
    public interface ITestResultMessage : ITestMessage
    {
        /// <summary>
        /// The execution time of the test, in seconds.
        /// </summary>
        decimal ExecutionTime { get; }

        /// <summary>
        /// The captured output of the test.
        /// </summary>
        string Output { get; }
    }
}