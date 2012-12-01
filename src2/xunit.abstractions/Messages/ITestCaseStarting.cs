namespace Xunit.Abstractions
{
    public interface ITestCaseStarting : ITestMessage
    {
        ITestCase TestCase { get; }
    }
}
