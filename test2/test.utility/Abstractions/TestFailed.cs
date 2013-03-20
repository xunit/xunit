using System;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestFailed : ITestFailed
{
    public TestFailed() : this(new Exception()) { }

    public TestFailed(Exception ex)
    {
        TestCase = new TestCase();
        ExceptionType = ex.GetType().FullName;
        Message = ExceptionUtility.GetMessage(ex);
        StackTrace = ExceptionUtility.GetStackTrace(ex);
    }

    public decimal ExecutionTime { get; set; }
    public ITestCase TestCase { get; set; }
    public string TestDisplayName { get; set; }
    public string ExceptionType { get; set; }
    public string Message { get; set; }
    public string StackTrace { get; set; }
}