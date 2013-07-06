using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCollectionStarting"/>.
    /// </summary>
    public class TestCollectionStarting : LongLivedMarshalByRefObject, ITestCollectionStarting
    {
        /// <inheritdoc/>
        public ITestCollection TestCollection { get; set; }
    }
}