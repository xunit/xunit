namespace Xunit.Abstractions
{
    /// <summary>
    /// Base message interface for all messages related to test classes.
    /// </summary>
    public interface ITestClassMessage : ITestCollectionMessage
    {
        /// <summary>
        /// The fully-qualified name of the test class.
        /// </summary>
        string ClassName { get; }
    }
}
