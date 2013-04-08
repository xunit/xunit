using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassFinished"/>.
    /// </summary>
    public class TestClassFinished : LongLivedMarshalByRefObject, ITestClassFinished
    {
        /// <inheritdoc/>
        public IAssemblyInfo Assembly { get; set; }

        /// <inheritdoc/>
        public string ClassName { get; set; }

        /// <inheritdoc/>
        public decimal ExecutionTime { get; set; }

        /// <inheritdoc/>
        public int TestsFailed { get; set; }

        /// <inheritdoc/>
        public int TestsRun { get; set; }

        /// <inheritdoc/>
        public int TestsSkipped { get; set; }
    }
}