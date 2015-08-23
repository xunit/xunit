using System.Linq;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestCaseMessage"/>.
    /// </summary>
    public class TestCaseMessage : TestMethodMessage, ITestCaseMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCaseMessage"/> class.
        /// </summary>
        public TestCaseMessage(ITestCase testCase)
            : base(testCase, testCase.TestMethod) { }

        /// <inheritdoc/>
        public ITestCase TestCase { get { return TestCases.FirstOrDefault(); } }
    }
}
