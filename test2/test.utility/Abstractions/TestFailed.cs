using System;
using Xunit.Abstractions;

public class TestFailed : ITestFailed
{
    public TestFailed()
    {
        TestCase = new TestCase();
        Exception = new Exception();
    }

    public Exception Exception { get; set; }
    public decimal ExecutionTime { get; set; }
    public ITestCase TestCase { get; set; }
    public string TestDisplayName { get; set; }
}