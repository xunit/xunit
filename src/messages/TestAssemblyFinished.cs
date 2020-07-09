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
            Guard.ArgumentNotNull(nameof(testCases), testCases);
            Guard.ArgumentNotNull(nameof(testAssembly), testAssembly);

            TestsSkipped = testsSkipped;
            TestsFailed = testsFailed;
            TestsRun = testsRun;
            ExecutionTime = executionTime;
        }

        /// <inheritdoc/>
        public decimal ExecutionTime { get; }

        /// <inheritdoc/>
        public int TestsFailed { get; }

        /// <inheritdoc/>
        public int TestsRun { get; }

        /// <inheritdoc/>
        public int TestsSkipped { get; }
    }
}
