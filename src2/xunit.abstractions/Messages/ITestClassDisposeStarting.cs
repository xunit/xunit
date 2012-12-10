namespace Xunit.Abstractions
{
    public interface ITestClassDisposeStarting : ITestMessage
    {
        ITestCase TestCase { get; }
        string TestDisplayName { get; }
    }
}
