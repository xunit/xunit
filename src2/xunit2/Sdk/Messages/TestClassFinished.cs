using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassFinished"/>.
    /// </summary>
    public class TestClassFinished : TestCollectionMessage, ITestClassFinished
    {
        public TestClassFinished(ITestCollection testCollection, string className, decimal executionTime, int testsRun, int testsFailed, int testsSkipped)
            : base(testCollection)
        {
            ClassName = className;
            ExecutionTime = executionTime;
            TestsRun = testsRun;
            TestsFailed = testsFailed;
            TestsSkipped = testsSkipped;
        }

        /// <inheritdoc/>
        public string ClassName { get; private set; }

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