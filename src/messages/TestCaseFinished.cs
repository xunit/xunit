using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestCaseFinished"/>.
    /// </summary>
    public class TestCaseFinished : TestCaseMessage, ITestCaseFinished
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCaseFinished"/> class.
        /// </summary>
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
