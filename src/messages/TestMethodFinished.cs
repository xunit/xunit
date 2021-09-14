using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestMethodFinished"/>.
    /// </summary>
    public class TestMethodFinished : TestMethodMessage, ITestMethodFinished
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethodFinished"/> class.
        /// </summary>
        public TestMethodFinished(IEnumerable<ITestCase> testCases, ITestMethod testMethod, decimal executionTime, int testsRun, int testsFailed, int testsSkipped)
            : base(testCases, testMethod)
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
