namespace Xunit.Abstractions
{
    public interface ITestSkipped : ITestResultMessage
    {
        string DisplayName { get; }
        string Reason { get; }
        ITestCase TestCase { get; }
    }
}
