namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test has finished executing.
    /// </summary>
    public interface ITestFinished : ITestMessage
    {
        /// <summary>
        /// The test case that this test belongs to.
        /// </summary>
        ITestCase TestCase { get; }

        /// <summary>
        /// The display name of the test.
        /// </summary>
        string TestDisplayName { get; }

        // TODO: How do we differentiate a test (when a test case has multiple tests)?
        // Is it solely by the display name of the test?
    }
}