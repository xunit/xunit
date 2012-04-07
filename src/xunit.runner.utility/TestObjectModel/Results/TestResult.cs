namespace Xunit
{
    /// <summary>
    /// Base class for all test results in the object model.
    /// </summary>
    public abstract class TestResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestResult"/> class.
        /// </summary>
        /// <param name="duration">The duration the test took to run. For skipped tests, should be 0.0.</param>
        /// <param name="displayName">The display name of the test result.</param>
        public TestResult(double duration, string displayName)
        {
            Duration = duration;
            DisplayName = displayName;
        }

        /// <summary>
        /// Gets the display name of the test result.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the duration the test took to run.
        /// </summary>
        public double Duration { get; private set; }
    }
}
