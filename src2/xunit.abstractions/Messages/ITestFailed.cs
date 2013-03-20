namespace Xunit.Abstractions
{
    public interface ITestFailed : ITestResultMessage
    {
        string ExceptionType { get; }
        string Message { get; }
        string StackTrace { get; }
    }
}