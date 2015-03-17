namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that the <see cref="System.IDisposable.Dispose"/> method is
    /// about to be called on the test class for the test case that just finished executing.
    /// </summary>
    public interface ITestClassDisposeStarting : ITestMessage, IExecutionMessage
    {
    }
}