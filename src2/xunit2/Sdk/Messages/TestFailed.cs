using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestFailed"/>.
    /// </summary>
    internal class TestFailed : TestResultMessage, ITestFailed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestFailed"/> class.
        /// </summary>
        public TestFailed(ITestCase testCase, string testDisplayName, decimal executionTime, string exceptionType, string message, string stackTrace)
            : base(testCase, testDisplayName, executionTime)
        {
            StackTrace = stackTrace;
            Message = message;
            ExceptionType = exceptionType;
        }

#if XUNIT2_DLL
        /// <summary>
        /// Initializes a new instance of the <see cref="TestFailed"/> class.
        /// </summary>
        /// <param name="ex">The exception that caused the test to fail.</param>
        public TestFailed(ITestCase testCase, string testDisplayName, decimal executionTime, Exception ex)
            : this(testCase, testDisplayName, executionTime, ex.GetType().FullName, ExceptionUtility.GetMessage(ex), ExceptionUtility.GetStackTrace(ex)) { }
#endif

        /// <inheritdoc/>
        public string ExceptionType { get; private set; }

        /// <inheritdoc/>
        public string Message { get; private set; }

        /// <inheritdoc/>
        public string StackTrace { get; private set; }
    }
}