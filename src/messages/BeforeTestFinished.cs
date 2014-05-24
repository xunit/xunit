using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="IBeforeTestFinished"/>.
    /// </summary>
    public class BeforeTestFinished : TestMessage, IBeforeTestFinished
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeforeTestFinished"/> class.
        /// </summary>
        public BeforeTestFinished(ITestCase testCase, string testDisplayName, string attributeName)
            : base(testCase, testDisplayName)
        {
            AttributeName = attributeName;
        }

        /// <inheritdoc/>
        public string AttributeName { get; private set; }
    }
}