namespace Xunit.Runners
{
    /// <summary>
    /// Represents test assembly execution being finished.
    /// </summary>
    public class ExecutionCompleteInfo
    {
        /// <summary/>
        public ExecutionCompleteInfo(int totalTests, int testsFailed, int testsSkipped, decimal executionTime)
        {
            TotalTests = totalTests;
            TestsFailed = testsFailed;
            TestsSkipped = testsSkipped;
            ExecutionTime = executionTime;
        }

        /// <summary>
        /// The total number of tests in the assembly.
        /// </summary>
        public int TotalTests { get; }

        /// <summary>
        /// The number of the tests that failed.
        /// </summary>
        public int TestsFailed { get; }

        /// <summary>
        /// The number of tests that were skipped.
        /// </summary>
        public int TestsSkipped { get; }

        /// <summary>
        /// The total execution time spent running tests.
        /// </summary>
        public decimal ExecutionTime { get; }

        /// <summary>
        /// Used to report results when no tests are executed.
        /// </summary>
        public static readonly ExecutionCompleteInfo Empty = new ExecutionCompleteInfo(0, 0, 0, 0M);
    }
}
