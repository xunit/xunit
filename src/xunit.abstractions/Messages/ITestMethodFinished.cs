namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test method has finished executing (meaning, all
    /// the test cases that derived from the test method have finished).
    /// </summary>
    public interface ITestMethodFinished : ITestMethodMessage, IExecutionMessage, IFinishedMessage
    {
    }
}