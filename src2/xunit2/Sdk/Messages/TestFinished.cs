using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestFinished"/>.
    /// </summary>
    public class TestFinished : LongLivedMarshalByRefObject, ITestFinished
    {
        /// <inheritdoc/>
        public string TestDisplayName { get; set; }

        /// <inheritdoc/>
        public decimal ExecutionTime { get; set; }

        /// <inheritdoc/>
        public ITestCase TestCase { get; set; }
    }
}