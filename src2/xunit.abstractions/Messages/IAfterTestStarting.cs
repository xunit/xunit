namespace Xunit.Abstractions
{
    public interface IAfterTestStarting : ITestMessage
    {
        string AttributeName { get; }
        ITestCase TestCase { get; }
        string TestDisplayName { get; }
    }
}
