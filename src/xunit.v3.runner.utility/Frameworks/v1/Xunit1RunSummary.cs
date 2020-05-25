namespace Xunit
{
    /// <summary>
    /// Collects statistics from running tests.
    /// </summary>
    public class Xunit1RunSummary
    {
        /// <summary>
        /// A flag that indicates whether or not to continue running tests.
        /// </summary>
        public bool Continue = true;

        /// <summary>
        /// The total number of tests run.
        /// </summary>
        public int Total;

        /// <summary>
        /// The number of tests that failed.
        /// </summary>
        public int Failed;

        /// <summary>
        /// The number of tests that were skipped.
        /// </summary>
        public int Skipped;

        /// <summary>
        /// The time spent running the tests.
        /// </summary>
        public decimal Time;

        /// <summary>
        /// Aggregates the current results with the other results.
        /// </summary>
        /// <param name="other">The other result.</param>
        public void Aggregate(Xunit1RunSummary other)
        {
            Total += other.Total;
            Failed += other.Failed;
            Skipped += other.Skipped;
            Time += other.Time;
            Continue &= other.Continue;
        }

        /// <summary>
        /// Resets the counted results back to zero.
        /// </summary>
        public void Reset()
        {
            Total = 0;
            Failed = 0;
            Skipped = 0;
            Time = 0M;
        }
    }
}
