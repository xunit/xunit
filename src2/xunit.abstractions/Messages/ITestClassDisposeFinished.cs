namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that the <see cref="System.IDisposable.Dispose"/> method was
    /// just called on the test class for the test case that just finished executing.
    /// </summary>
    public interface ITestClassDisposeFinished : ITestMessage
    {
        /// <summary>
        /// The test case that just finished executing.
        /// </summary>
        ITestCase TestCase { get; }

        /// <summary>
        /// The display name of the test case.
        /// </summary>
        string TestDisplayName { get; }
    }
}