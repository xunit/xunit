using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassDisposeFinished"/>.
    /// </summary>
    public class TestClassDisposeFinished : LongLivedMarshalByRefObject, ITestClassDisposeFinished
    {
        /// <inheritdoc/>
        public ITestCase TestCase { get; set; }

        /// <inheritdoc/>
        public string TestDisplayName { get; set; }
    }
}