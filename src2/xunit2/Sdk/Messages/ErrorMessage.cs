using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IErrorMessage"/>.
    /// </summary>
    public class ErrorMessage : LongLivedMarshalByRefObject, IErrorMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        public ErrorMessage(string exceptionType, string message, string stackTrace)
        {
            StackTrace = stackTrace;
            Message = message;
            ExceptionType = exceptionType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
        /// </summary>
        /// <param name="ex">The exception that represents the error message.</param>
        public ErrorMessage(Exception ex)
            : this(ex.GetType().FullName, ExceptionUtility.GetMessage(ex), ExceptionUtility.GetStackTrace(ex)) { }

        /// <inheritdoc/>
        public string ExceptionType { get; private set; }

        /// <inheritdoc/>
        public string Message { get; private set; }

        /// <inheritdoc/>
        public string StackTrace { get; private set; }
    }
}