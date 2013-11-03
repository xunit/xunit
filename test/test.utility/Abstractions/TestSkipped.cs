using Xunit.Abstractions;

public class TestSkipped : ITestSkipped
{
    public TestSkipped()
    {
        TestCase = new TestCase();
        TestCollection = new TestCollection();
    }

    public decimal ExecutionTime { get; set; }
    public string Reason { get; set; }
    public ITestCase TestCase { get; set; }
    public ITestCollection TestCollection { get; set; }
    public string TestDisplayName { get; set; }
}