namespace Xunit.Runners
{
    /// <summary>
    /// Represents test discovery being completed.
    /// </summary>
    public class DiscoveryCompleteInfo
    {
        /// <summary/>
        public DiscoveryCompleteInfo(int testCasesDiscovered, int testCasesToRun)
        {
            TestCasesDiscovered = testCasesDiscovered;
            TestCasesToRun = testCasesToRun;
        }

        /// <summary>
        /// The number of test cases that were discovered.
        /// </summary>
        public int TestCasesDiscovered { get; }

        /// <summary>
        /// The number of test cases that will be run, after filtering was applied.
        /// </summary>
        public int TestCasesToRun { get; }
    }
}
