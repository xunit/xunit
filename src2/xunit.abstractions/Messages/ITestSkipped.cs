namespace Xunit.Abstractions
{
    public interface ITestSkipped : ITestResultMessage
    {
        string Reason { get; }
    }
}
