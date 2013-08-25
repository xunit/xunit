using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCollectionFinished"/>.
    /// </summary>
    public class TestCollectionFinished : TestCollectionMessage, ITestCollectionFinished
    {
        public TestCollectionFinished(ITestCollection testCollection, decimal executionTime, int testsRun, int testsFailed, int testsSkipped)
            : base(testCollection)
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