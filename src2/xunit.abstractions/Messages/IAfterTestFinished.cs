namespace Xunit.Abstractions
{
    public interface IAfterTestFinished : ITestMessage
    {
        string AttributeName { get; }
        ITestCase TestCase { get; }
        string TestDisplayName { get; }
    }
}
