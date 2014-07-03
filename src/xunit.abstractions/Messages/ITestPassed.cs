namespace Xunit.Abstractions
{
    /// <summary>
    /// Indicates that a test has passed.
    /// </summary>
    public interface ITestPassed : ITestResultMessage, IExecutionMessage
    {
    }
}