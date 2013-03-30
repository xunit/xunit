namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a skipped test of <see cref="ITestCaseResult"/>.
    /// </summary>
    public interface ISkippedTestCaseResult : ITestCaseResult
    {
        /// <summary>
        /// The reason that was indicated for skipping the test
        /// </summary>
        string Reason { get; }
    }
}
