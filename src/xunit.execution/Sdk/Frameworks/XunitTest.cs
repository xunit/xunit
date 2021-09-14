using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// An implementation of <see cref="ITest"/> for xUnit v2.
    /// </summary>
    public class XunitTest : LongLivedMarshalByRefObject, ITest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTest"/> class.
        /// </summary>
        /// <param name="testCase">The test case this test belongs to.</param>
        /// <param name="displayName">The display name for this test.</param>
        public XunitTest(IXunitTestCase testCase, string displayName)
        {
            TestCase = testCase;
            DisplayName = displayName;
        }

        /// <inheritdoc/>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the xUnit v2 test case.
        /// </summary>
        public IXunitTestCase TestCase { get; private set; }

        /// <inheritdoc/>
        ITestCase ITest.TestCase { get { return TestCase; } }
    }
}
