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
        Output = String.Empty;

        var failureInfo = ExceptionUtility.ConvertExceptionToFailureInformation(ex);
        ExceptionParentIndices = failureInfo.ExceptionParentIndices;
        ExceptionTypes = failureInfo.ExceptionTypes;
        Messages = failureInfo.Messages;
        StackTraces = failureInfo.StackTraces;
    }

    public int[] ExceptionParentIndices { get; set; }
    public string[] ExceptionTypes { get; set; }
    public decimal ExecutionTime { get; set; }
    public string[] Messages { get; set; }
    public string Output { get; set; }
    public string[] StackTraces { get; set; }
    public ITestCase TestCase { get; set; }
    public ITestCollection TestCollection { get; set; }
    public string TestDisplayName { get; set; }

    public void Dispose() { }
}