namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test collection has just finished executing (meaning,
    /// all the test classes in the collection has finished).
    /// </summary>
    public interface ITestCollectionFinished : IFinishedMessage
    {
        /// <summary>
        /// Gets the test collection.
        /// </summary>
        ITestCollection TestCollection { get; }
    }
}