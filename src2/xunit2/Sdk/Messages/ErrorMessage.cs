using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class ErrorMessage : LongLivedMarshalByRefObject, IErrorMessage
    {
        public Exception Error { get; internal set; }
    }
}
