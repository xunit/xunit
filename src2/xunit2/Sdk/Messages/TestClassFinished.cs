using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassFinished"/>.
    /// </summary>
    internal class TestClassFinished : TestClassMessage, ITestClassFinished
    {
        public TestClassFinished(ITestCollection testCollection, string className, decimal executionTime, int testsRun, int testsFailed, int testsSkipped)
            : base(testCollection, className)
        {
            ExecutionTime = executionTime;
            TestsRun = testsRun;
            TestsFailed = testsFailed;
            TestsSkipped = testsSkipped;
        }

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