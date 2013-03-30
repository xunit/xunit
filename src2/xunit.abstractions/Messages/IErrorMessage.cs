namespace Xunit.Abstractions
{
    /// <summary>
    /// This message when sent indicates that an error has occured in the 
    /// execution process. 
    /// </summary>
    public interface IErrorMessage : ITestMessage
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
