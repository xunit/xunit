using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="IBeforeTestStarting"/>.
    /// </summary>
    public class BeforeTestStarting : TestMessage, IBeforeTestStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeforeTestStarting"/> class.
        /// </summary>
        public BeforeTestStarting(ITestCase testCase, string testDisplayName, string attributeName)
            : base(testCase, testDisplayName)
        {
            AttributeName = attributeName;
        }

        /// <inheritdoc/>
        public string AttributeName { get; private set; }
    }
}