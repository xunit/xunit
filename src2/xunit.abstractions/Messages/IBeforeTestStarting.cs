namespace Xunit.Abstractions
{
    public interface IBeforeTestStarting : ITestMessage
    {
        string AttributeName { get; }
        ITestCase TestCase { get; }
        string TestDisplayName { get; }
    }
}
