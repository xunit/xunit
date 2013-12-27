using System;
using Xunit.Abstractions;

public class TestSkipped : ITestSkipped
{
    public TestSkipped()
    {
        TestCase = new TestCase();
        TestCollection = new TestCollection();
        Output = String.Empty;
    }

    public decimal ExecutionTime { get; set; }
    public string Output { get; set; }
    public string Reason { get; set; }
    public ITestCase TestCase { get; set; }
    public ITestCollection TestCollection { get; set; }
    public string TestDisplayName { get; set; }

    public void Dispose() { }
}