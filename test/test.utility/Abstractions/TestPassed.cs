using Xunit.Abstractions;

public class TestPassed : ITestPassed
{
    public TestPassed()
    {
        TestCase = new TestCase();
        TestCollection = new TestCollection();
    }

    public decimal ExecutionTime { get; set; }
    public ITestCase TestCase { get; set; }
    public ITestCollection TestCollection { get; set; }
    public string TestDisplayName { get; set; }
}