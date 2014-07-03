namespace Xunit.Abstractions
{
    /// <summary>
    /// Base message interface for all messages related to test collections.
    /// </summary>
    public interface ITestCollectionMessage : ITestAssemblyMessage
    {
        /// <summary>
        /// The test collection that is associated with this message.
        /// </summary>
        ITestCollection TestCollection { get; }
    }
}