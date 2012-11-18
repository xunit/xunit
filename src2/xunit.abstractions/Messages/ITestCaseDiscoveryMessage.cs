namespace Xunit.Abstractions
{
    public interface ITestCaseDiscoveryMessage : ITestMessage
    {
        ITestCase TestCase { get; }
    }
}
