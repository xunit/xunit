namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test case had been found during the discovery process.
    /// </summary>
    public interface ITestCaseDiscoveryMessage : ITestMessage
    {
        /// <summary>
        /// The test case that was discovered.
        /// </summary>
        ITestCase TestCase { get; }
    }
}