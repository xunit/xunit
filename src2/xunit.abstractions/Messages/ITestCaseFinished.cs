namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents the test case scope of <see cref="IFinishedMessage"/>. When 
    /// this message is received all the tests that were being run for this 
    /// test case are completed.
    /// </summary>
    public interface ITestCaseFinished : IFinishedMessage
    {
        /// <summary>
        /// The TestCase that has been completed
        /// </summary>
        ITestCase TestCase { get; }
    }
}
