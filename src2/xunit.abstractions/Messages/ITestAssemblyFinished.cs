namespace Xunit.Abstractions
{
    /// <summary>
    /// c. When 
    /// this message is received all the tests that were being run for this 
    /// assembly are completed.
    /// </summary>
    public interface ITestAssemblyFinished : IFinishedMessage
    {
    }
}
