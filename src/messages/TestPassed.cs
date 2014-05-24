using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestPassed"/>.
    /// </summary>
    public class TestPassed : TestResultMessage, ITestPassed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestPassed"/> class.
        /// </summary>
        public TestPassed(ITestCase testCase, string testDisplayName, decimal executionTime, string output)
            : base(testCase, testDisplayName, executionTime, output) { }
    }
}