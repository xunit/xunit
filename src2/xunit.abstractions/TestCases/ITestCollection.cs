namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a group of test cases. Test collections form the basis of the parallelization in
    /// xUnit.net. Test cases which are in the same test collection will not be run in parallel
    /// against sibling tests, but will run in parallel against tests in other collections.
    /// </summary>
    public interface ITestCollection
    {
        string DisplayName { get; }
    }
}
