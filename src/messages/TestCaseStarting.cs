using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestCaseStarting"/>.
    /// </summary>
    public class TestCaseStarting : TestCaseMessage, ITestCaseStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCaseStarting"/> class.
        /// </summary>
        public TestCaseStarting(ITestCase testCase)
            : base(testCase) { }
    }
}
