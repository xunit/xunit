namespace Xunit.Abstractions
{
    public interface ITestMethodFinished : ITestMessage
    {
        ITestCase TestCase { get; }
        string TestDisplayName { get; }
    }
}
