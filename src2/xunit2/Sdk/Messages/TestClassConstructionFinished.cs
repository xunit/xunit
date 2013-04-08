using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassConstructionFinished"/>.
    /// </summary>
    public class TestClassConstructionFinished : LongLivedMarshalByRefObject, ITestClassConstructionFinished
    {
        /// <inheritdoc/>
        public ITestCase TestCase { get; set; }

        /// <inheritdoc/>
        public string TestDisplayName { get; set; }
    }
}