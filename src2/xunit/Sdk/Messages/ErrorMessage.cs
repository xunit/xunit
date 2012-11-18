using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    internal class ErrorMessage : LongLivedMarshalByRefObject, IErrorMessage
    {
        public Exception Error { get; internal set; }
    }
}
