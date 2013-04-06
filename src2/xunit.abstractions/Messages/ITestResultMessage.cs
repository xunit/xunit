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
        /// The test case that this test is associated with.
        /// </summary>
        ITestCase TestCase { get; }

        /// <summary>
        /// The display name of the test.
        /// </summary>
        string TestDisplayName { get; }
    }
}