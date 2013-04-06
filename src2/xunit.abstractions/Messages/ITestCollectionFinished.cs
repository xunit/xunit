namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test collection has just finished executing (meaning,
    /// all the test classes in the collection has finished).
    /// </summary>
    public interface ITestCollectionFinished : IFinishedMessage
    {
        // TODO: How do we represent a collection?
    }
}