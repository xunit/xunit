using System;

namespace Xunit.Abstractions
{
    /// <summary>
    /// The IFailedTestCaseResult is sent during the execution process and 
    /// it indicates that the test case that is being executed has failed. 
    /// </summary>
    public interface IFailedTestCaseResult : ITestCaseResult
    {
        /// <summary>
        /// The exception that caused the failure. 
        /// </summary>
        /// <returns>An <see cref="Exception" /> that caused the test case to fail</returns> 
        Exception Exception { get; }
    }
}
