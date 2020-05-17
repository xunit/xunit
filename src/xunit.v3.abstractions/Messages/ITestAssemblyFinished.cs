namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that the execution process has been completed for
    /// the requested assembly.
    /// </summary>
    public interface ITestAssemblyFinished : ITestAssemblyMessage, IExecutionMessage, IFinishedMessage
    {
    }
}
