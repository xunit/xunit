namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that an error has occurred in the execution process. 
    /// </summary>
    public interface IErrorMessage : ITestMessage
    {
        /// <summary>
        /// The fully-qualified type name of the exception.
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