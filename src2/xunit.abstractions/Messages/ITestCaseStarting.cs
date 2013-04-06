namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test case is about to start executing.
    /// </summary>
    public interface ITestCaseStarting : ITestMessage
    {
        /// <summary>
        /// The test case that is about to execute.
        /// </summary>
        ITestCase TestCase { get; }
    }
}