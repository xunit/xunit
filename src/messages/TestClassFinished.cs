using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassFinished"/>.
    /// </summary>
    public class TestClassFinished : TestClassMessage, ITestClassFinished
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassFinished"/> class.
        /// </summary>
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