namespace Xunit.Abstractions
{
    /// <summary>
    /// Base message interface for all messages related to test classes.
    /// </summary>
    public interface ITestClassMessage : ITestCollectionMessage
    {
        /// <summary>
        /// The test class that is associated with this message.
        /// </summary>
        ITestClass TestClass { get; }
    }
}
