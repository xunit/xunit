namespace Xunit.Sdk
{
    /// <summary>
    /// Represents the statistical summary from a run of one or more tests.
    /// </summary>
    public class RunSummary
    {
        /// <summary>
        /// The total number of tests run.
        /// </summary>
        public int Total;

        /// <summary>
        /// The number of failed tests.
        /// </summary>
        public int Failed;

        /// <summary>
        /// The number of skipped tests.
        /// </summary>
        public int Skipped;

        /// <summary>
        /// The total time taken to run the tests, in seconds.
        /// </summary>
        public decimal Time;

        /// <summary>
        /// Adds a run summary's totals into this run summary.
        /// </summary>
        /// <param name="other">The run summary to be added.</param>
        public void Aggregate(RunSummary other)
        {
            Total += other.Total;
            Failed += other.Failed;
            Skipped += other.Skipped;
            Time += other.Time;
        }
    }
}
