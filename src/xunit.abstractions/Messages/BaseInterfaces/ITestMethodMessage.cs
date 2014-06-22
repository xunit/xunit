namespace Xunit.Abstractions
{
    /// <summary>
    /// Base message interface for all messages related to test methods.
    /// </summary>
    public interface ITestMethodMessage : ITestClassMessage
    {
        /// <summary>
        /// The test method that is associated with this message.
        /// </summary>
        ITestMethod TestMethod { get; }
    }
}
