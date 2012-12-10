namespace Xunit.Abstractions
{
    public interface ITestMethodStarting : ITestMessage
    {
        ITestCase TestCase { get; }
        string TestDisplayName { get; }
    }
}
