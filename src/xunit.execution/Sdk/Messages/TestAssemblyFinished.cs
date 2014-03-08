using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyFinished"/>.
    /// </summary>
    internal class TestAssemblyFinished : LongLivedMarshalByRefObject, ITestAssemblyFinished
    {
        public TestAssemblyFinished(IAssemblyInfo assembly, decimal executionTime, int testsRun, int testsFailed, int testsSkipped)
        {
            TestsSkipped = testsSkipped;
            TestsFailed = testsFailed;
            TestsRun = testsRun;
            ExecutionTime = executionTime;
            Assembly = assembly;
        }

        /// <inheritdoc/>
        public IAssemblyInfo Assembly { get; private set; }

        /// <inheritdoc/>
        public decimal ExecutionTime { get; private set; }

        /// <inheritdoc/>
        public int TestsFailed { get; private set; }

        /// <inheritdoc/>
        public int TestsRun { get; private set; }

        /// <inheritdoc/>
        public int TestsSkipped { get; private set; }
    }
}