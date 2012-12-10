namespace Xunit.Abstractions
{
    public interface IBeforeTestFinished : ITestMessage
    {
        string AttributeName { get; }
        ITestCase TestCase { get; }
        string TestDisplayName { get; }
    }
}
