namespace Xunit.Runners
{
    /// <summary>
    /// Represents an error that happened outside the scope of a running test.
    /// </summary>
    public class ErrorMessageInfo
    {
        /// <summary/>
        public ErrorMessageInfo(ErrorMessageType messageType, string exceptionType, string exceptionMessage, string exceptionStackTrace)
        {
            MesssageType = messageType;
            ExceptionType = exceptionType;
            ExceptionMessage = exceptionMessage;
            ExceptionStackTrace = exceptionStackTrace;
        }

        /// <summary>
        /// The type of error condition that was encountered.
        /// </summary>
        public ErrorMessageType MesssageType { get; }

        /// <summary>
        /// The exception that caused the test failure.
        /// </summary>
        public string ExceptionType { get; }

        /// <summary>
        /// The message from the exception that caused the test failure.
        /// </summary>
        public string ExceptionMessage { get; }

        /// <summary>
        /// The stack trace from the exception that caused the test failure.
        /// </summary>
        public string ExceptionStackTrace { get; }
    }
}
