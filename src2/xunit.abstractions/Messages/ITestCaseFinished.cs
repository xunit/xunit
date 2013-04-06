namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test case has finished executing.
    /// </summary>
    public interface ITestCaseFinished : IFinishedMessage
    {
        /// <summary>
        /// The test case that has finished executing.
        /// </summary>
        ITestCase TestCase { get; }
    }
}