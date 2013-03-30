using System;

namespace Xunit.Abstractions
{
    /// <summary>
    /// This message when sent indicates that an error has occured in the 
    /// execution process. 
    /// </summary>
    public interface IErrorMessage : ITestMessage
    {
        string ExceptionType { get; }
        string Message { get; }
        string StackTrace { get; }
    }
}
