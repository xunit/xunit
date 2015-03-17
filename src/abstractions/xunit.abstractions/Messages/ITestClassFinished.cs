namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test class has finished executing (meaning, all of the
    /// test cases in this test class have finished running).
    /// </summary>
    public interface ITestClassFinished : ITestClassMessage, IExecutionMessage, IFinishedMessage
    {
    }
}