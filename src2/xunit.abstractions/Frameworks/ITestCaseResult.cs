namespace Xunit.Abstractions
{
    /// <summary>
    /// This represents the test result of executing a test case. 
    /// </summary>
    public interface ITestCaseResult
    {
        /// <summary>
        /// The total time of execution for this test case in milli-seconds. 
        /// </summary>
        double ExecutionTime { get; }

        /// <summary>
        /// Any output that was sent to the Console during execution
        /// </summary>
        string Output { get; }

        /// <summary>
        /// The test case that was being run. 
        /// </summary>
        ITestCase TestCase { get; }
    }
}
