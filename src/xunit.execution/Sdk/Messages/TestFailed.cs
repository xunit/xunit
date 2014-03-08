using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestFailed"/>.
    /// </summary>
    internal class TestFailed : TestResultMessage, ITestFailed
    {
        public TestFailed(ITestCase testCase, string testDisplayName, decimal executionTime, string output, string exceptionType, string message, string stackTrace)
            : base(testCase, testDisplayName, executionTime, output)
        {
            StackTrace = stackTrace;
            Message = message;
            ExceptionType = exceptionType;
        }

#if XUNIT_CORE_DLL
        public TestFailed(ITestCase testCase, string testDisplayName, decimal executionTime, string output, Exception ex)
            : this(testCase, testDisplayName, executionTime, output, ex.GetType().FullName, ExceptionUtility.GetMessage(ex), ExceptionUtility.GetStackTrace(ex)) { }
#endif

        /// <inheritdoc/>
        public string ExceptionType { get; private set; }

        /// <inheritdoc/>
        public string Message { get; private set; }

        /// <inheritdoc/>
        public string StackTrace { get; private set; }
    }
}