using Xunit.Abstractions;

public class TestPassed : ITestPassed
{
    public TestPassed()
    {
        TestCase = new TestCase();
    }

    public decimal ExecutionTime { get; set; }
    public ITestCase TestCase { get; set; }
    public string TestDisplayName { get; set; }
}