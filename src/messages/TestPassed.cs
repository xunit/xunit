using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
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
        public TestPassed(ITest test, decimal executionTime, string output)
            : base(test, executionTime, output) { }
    }
}
