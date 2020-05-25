using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="ITest"/> for xUnit v1.
    /// </summary>
    public class Xunit1Test : LongLivedMarshalByRefObject, ITest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit1Test"/> class.
        /// </summary>
        /// <param name="testCase">The test case this test belongs to.</param>
        /// <param name="displayName">The display name for this test.</param>
        public Xunit1Test(ITestCase testCase, string displayName)
        {
            TestCase = testCase;
            DisplayName = displayName;
        }

        /// <inheritdoc/>
        public string DisplayName { get; private set; }

        /// <inheritdoc/>
        public ITestCase TestCase { get; private set; }
    }
}
