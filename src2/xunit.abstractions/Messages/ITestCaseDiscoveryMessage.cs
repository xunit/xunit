namespace Xunit.Abstractions
{
    /// <summary>
    /// The ITestCaseDiscoveryMessage is sent during the Discovery process 
    /// to indicate that a new test case has been found. 
    /// </summary>
    public interface ITestCaseDiscoveryMessage : ITestMessage
    {
        /// <summary>
        /// The test case that has been found
        /// </summary>
        ITestCase TestCase { get; }
    }
}
