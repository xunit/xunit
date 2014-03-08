using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCollectionStarting"/>.
    /// </summary>
    internal class TestCollectionStarting : TestCollectionMessage, ITestCollectionStarting
    {
        public TestCollectionStarting(ITestCollection testCollection)
            : base(testCollection) { }
    }
}