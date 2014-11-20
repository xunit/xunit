namespace Xunit.Abstractions
{
    /// <summary>
    /// Base message interface for all messages related to tests.
    /// </summary>
    public interface ITestMessage : ITestCaseMessage
    {
        /// <summary>
        /// The test that is associated with this message.
        /// </summary>
        ITest Test { get; }
    }
}
