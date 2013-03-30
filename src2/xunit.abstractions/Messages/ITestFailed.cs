namespace Xunit.Abstractions
{
    /// <summary>
    /// This message when sent indicates a test has failed.
    /// </summary>
    public interface ITestFailed : ITestResultMessage
    {
        /// <summary>
        /// The CLR type name of the exception.
        /// </summary>
        string ExceptionType { get; }

        /// <summary>
        /// The message of the exception.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// The stack trace of the exception.
        /// </summary>
        string StackTrace { get; }
    }
}