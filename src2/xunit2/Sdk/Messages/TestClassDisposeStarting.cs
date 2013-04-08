using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassDisposeStarting"/>.
    /// </summary>
    public class TestClassDisposeStarting : LongLivedMarshalByRefObject, ITestClassDisposeStarting
    {
        /// <inheritdoc/>
        public ITestCase TestCase { get; set; }

        /// <inheritdoc/>
        public string TestDisplayName { get; set; }
    }
}