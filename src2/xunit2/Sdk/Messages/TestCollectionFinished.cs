using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCollectionFinished"/>.
    /// </summary>
    public class TestCollectionFinished : LongLivedMarshalByRefObject, ITestCollectionFinished
    {
        /// <inheritdoc/>
        public IAssemblyInfo Assembly { get; set; }

        /// <inheritdoc/>
        public decimal ExecutionTime { get; set; }

        /// <inheritdoc/>
        public ITestCollection TestCollection { get; set; }

        /// <inheritdoc/>
        public int TestsFailed { get; set; }

        /// <inheritdoc/>
        public int TestsRun { get; set; }

        /// <inheritdoc/>
        public int TestsSkipped { get; set; }
    }
}