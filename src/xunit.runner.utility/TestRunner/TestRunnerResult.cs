namespace Xunit
{
    /// <summary>
    /// The result of a test run via <see cref="TestRunner"/>.
    /// </summary>
    public enum TestRunnerResult
    {
        /// <summary>
        /// All tests passed, with no class-level failures
        /// </summary>
        Passed,
        /// <summary>
        /// At least one test failed, or there was a class-level failure
        /// </summary>
        Failed,
        /// <summary>
        /// There were no tests to run
        /// </summary>
        NoTests
    }
}