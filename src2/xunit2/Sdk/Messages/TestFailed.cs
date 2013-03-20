using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestFailed : TestResultMessage, ITestFailed
    {
        public TestFailed() { }

        public TestFailed(Exception ex)
        {
            ExceptionType = ex.GetType().FullName;
            Message = ExceptionUtility.GetMessage(ex);
            StackTrace = ExceptionUtility.GetStackTrace(ex);
        }

        public string ExceptionType { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }
    }
}