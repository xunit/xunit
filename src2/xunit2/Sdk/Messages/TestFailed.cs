using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestFailed"/>.
    /// </summary>
    public class TestFailed : TestResultMessage, ITestFailed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestFailed"/> class.
        /// </summary>
        public TestFailed() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestFailed"/> class.
        /// </summary>
        /// <param name="ex">The exception that caused the test to fail.</param>
        public TestFailed(Exception ex)
        {
            ExceptionType = ex.GetType().FullName;
            Message = ExceptionUtility.GetMessage(ex);
            StackTrace = ExceptionUtility.GetStackTrace(ex);
        }

        /// <inheritdoc/>
        public string ExceptionType { get; set; }

        /// <inheritdoc/>
        public string Message { get; set; }

        /// <inheritdoc/>
        public string StackTrace { get; set; }
    }
}