using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCaseFinished"/>.
    /// </summary>
    public class TestCaseFinished : LongLivedMarshalByRefObject, ITestCaseFinished
    {
        /// <inheritdoc/>
        public IAssemblyInfo Assembly { get; set; }

        /// <inheritdoc/>
        public decimal ExecutionTime { get; set; }

        /// <inheritdoc/>
        public ITestCase TestCase { get; set; }

        /// <inheritdoc/>
        public int TestsFailed { get; set; }

        /// <inheritdoc/>
        public int TestsRun { get; set; }

        /// <inheritdoc/>
        public int TestsSkipped { get; set; }
    }
}