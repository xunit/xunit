using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyFinished"/>.
    /// </summary>
    public class TestAssemblyFinished : LongLivedMarshalByRefObject, ITestAssemblyFinished
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyFinished"/> class.
        /// </summary>
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