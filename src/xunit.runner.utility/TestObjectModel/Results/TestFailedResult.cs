namespace Xunit
{
    /// <summary>
    /// Represents a failed test run in the object model.
    /// </summary>
    public class TestFailedResult : TestResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestFailedResult"/> class.
        /// </summary>
        /// <param name="duration">The duration the test took to run.</param>
        /// <param name="displayName">The display name of the test result.</param>
        /// <param name="output">The output that was captured during the test run.</param>
        /// <param name="exceptionType">Type of the exception.</param>
        /// <param name="exceptionMessage">The exception message.</param>
        /// <param name="exceptionStackTrace">The exception stack trace.</param>
        public TestFailedResult(double duration, string displayName, string output, string exceptionType,
                                string exceptionMessage, string exceptionStackTrace)
            : base(duration, displayName)
        {
            Output = output;
            ExceptionType = exceptionType;
            ExceptionMessage = exceptionMessage;
            ExceptionStackTrace = exceptionStackTrace;
        }

        /// <summary>
        /// Gets the output that was captured during the test run.
        /// </summary>
        public string Output { get; private set; }

        /// <summary>
        /// Gets the type of the exception.
        /// </summary>
        public string ExceptionType { get; private set; }

        /// <summary>
        /// Gets the exception message.
        /// </summary>
        public string ExceptionMessage { get; private set; }

        /// <summary>
        /// Gets the exception stack trace.
        /// </summary>
        public string ExceptionStackTrace { get; private set; }
    }
}