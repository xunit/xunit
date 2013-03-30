using System;

namespace Xunit.Abstractions
{
    /// <summary>
    /// This message when sent indicates that an error has occured in the 
    /// execution process. 
    /// </summary>
    public interface IErrorMessage : ITestMessage
    {
        /// <summary>
        /// The associated exception that caused the error. 
        /// </summary>
        /// <returns>An <see cref="Exception" /> associated with the Error</returns> 
        Exception Error { get; }
    }
}
