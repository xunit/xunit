namespace Xunit
{
    /// <summary>
    /// Represents a passed test run in the object model.
    /// </summary>
    public class TestPassedResult : TestResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestPassedResult"/> class.
        /// </summary>
        /// <param name="duration">The duration the test took to run.</param>
        /// <param name="displayName">The display name of the test result.</param>
        /// <param name="output">The output that was captured during the test run.</param>
        public TestPassedResult(double duration, string displayName, string output)
            : base(duration, displayName)
        {
            Output = output;
        }

        /// <summary>
        /// Gets the output that was captured during the test run.
        /// </summary>
        public string Output { get; private set; }
    }
}