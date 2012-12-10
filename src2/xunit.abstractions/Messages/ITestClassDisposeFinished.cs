namespace Xunit.Abstractions
{
    public interface ITestClassDisposeFinished : ITestMessage
    {
        ITestCase TestCase { get; }
        string TestDisplayName { get; }
    }
}
