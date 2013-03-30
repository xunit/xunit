using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <inheritdoc />
    public class ErrorMessage : LongLivedMarshalByRefObject, IErrorMessage
    {
        public ErrorMessage() { }

        public ErrorMessage(Exception ex)
        {
            ExceptionType = ex.GetType().FullName;
            Message = ExceptionUtility.GetMessage(ex);
            StackTrace = ExceptionUtility.GetStackTrace(ex);
        }

        public string ExceptionType { get; private set; }
        public string Message { get; private set; }
        public string StackTrace { get; private set; }
    }
}
