using System;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TestFailed : ITestFailed
{
    public TestFailed() : this(new Exception()) { }

    public TestFailed(Exception ex)
    {
        TestCase = new TestCase();
        TestCollection = new TestCollection();
        ExceptionType = ex.GetType().FullName;
        Output = String.Empty;
        Message = ExceptionUtility.GetMessage(ex);
        StackTrace = ExceptionUtility.GetStackTrace(ex);
    }

    public string ExceptionType { get; set; }
    public decimal ExecutionTime { get; set; }
    public string Message { get; set; }
    public string Output { get; set; }
    public string StackTrace { get; set; }
    public ITestCase TestCase { get; set; }
    public ITestCollection TestCollection { get; set; }
    public string TestDisplayName { get; set; }

    public void Dispose() { }
}