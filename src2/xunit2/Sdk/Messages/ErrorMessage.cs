using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <inheritdoc />
    public class ErrorMessage : LongLivedMarshalByRefObject, IErrorMessage
    {
        public Exception Error { get; internal set; }
    }
}
