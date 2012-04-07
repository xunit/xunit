namespace Xunit
{
    /// <summary>
    /// Represents a skipped test run in the object model.
    /// </summary>
    public class TestSkippedResult : TestResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestSkippedResult"/> class.
        /// </summary>
        /// <param name="displayName">The display name of the test result.</param>
        /// <param name="reason">The skip reason.</param>
        public TestSkippedResult(string displayName, string reason)
            : base(0.0, displayName)
        {
            Reason = reason;
        }

        /// <summary>
        /// Gets the skip reason.
        /// </summary>
        public string Reason { get; private set; }
    }
}