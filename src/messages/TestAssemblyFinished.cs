using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyFinished"/>.
    /// </summary>
    public class TestAssemblyFinished : TestAssemblyMessage, ITestAssemblyFinished
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyFinished"/> class.
        /// </summary>
        public TestAssemblyFinished(IEnumerable<ITestCase> testCases, ITestAssembly testAssembly, decimal executionTime, int testsRun, int testsFailed, int testsSkipped)
            : base(testCases, testAssembly)
        {
            TestsSkipped = testsSkipped;
            TestsFailed = testsFailed;
            TestsRun = testsRun;
            ExecutionTime = executionTime;
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
