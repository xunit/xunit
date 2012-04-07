namespace Xunit
{
    /// <summary>
    /// Indicates the composite test method status
    /// </summary>
    public enum TestStatus
    {
        /// <summary>
        /// The method has not been run
        /// </summary>
        NotRun,
        /// <summary>
        /// All test results for the last run passed
        /// </summary>
        Passed,
        /// <summary>
        /// At least one test result for the last run failed
        /// </summary>
        Failed,
        /// <summary>
        /// At least one test result for the last run was skipped, and none failed
        /// </summary>
        Skipped
    }
}
