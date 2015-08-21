using System.Collections.Generic;

namespace Xunit.Runners
{
    /// <summary>
    /// Represents a test that failed.
    /// </summary>
    public class TestFailedInfo : TestExecutedInfo
    {
        /// <summary/>
        public TestFailedInfo(string typeName,
                              string methodName,
                              Dictionary<string, List<string>> traits,
                              string testDisplayName,
                              string testCollectionDisplayName,
                              decimal executionTime,
                              string output,
                              string exceptionType,
                              string exceptionMessage,
                              string exceptionStackTrace)
            : base(typeName, methodName, traits, testDisplayName, testCollectionDisplayName, executionTime, output)
        {
            ExceptionType = exceptionType;
            ExceptionMessage = exceptionMessage;
            ExceptionStackTrace = exceptionStackTrace;
        }

        /// <summary>
        /// The exception that caused the test failure.
        /// </summary>
        public string ExceptionType { get; }

        /// <summary>
        /// The message from the exception that caused the test failure.
        /// </summary>
        public string ExceptionMessage { get; }

        /// <summary>
        /// The stack trace from the exception that caused the test failure.
        /// </summary>
        public string ExceptionStackTrace { get; }
    }
}
