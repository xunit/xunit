namespace Xunit.Abstractions
{
    public interface ITestStarting : ITestMessage
    {
        ITestCase TestCase { get; }

        /// <summary>
        /// Gets the display name of the test.
        /// </summary>
        string DisplayName { get; }

        // TODO: How do we differentiate a test?
    }
}
