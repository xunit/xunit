using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCaseFinished"/>.
    /// </summary>
    internal class TestCaseFinished : TestCaseMessage, ITestCaseFinished
    {
        public TestCaseFinished(ITestCase testCase, decimal executionTime, int testsRun, int testsFailed, int testsSkipped)
            : base(testCase)
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