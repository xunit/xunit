using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="IAfterTestStarting"/>.
    /// </summary>
    public class AfterTestStarting : TestMessage, IAfterTestStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AfterTestStarting"/> class.
        /// </summary>
        public AfterTestStarting(ITestCase testCase, string testDisplayName, string attributeName)
            : base(testCase, testDisplayName)
        {
            AttributeName = attributeName;
        }

        /// <inheritdoc/>
        public string AttributeName { get; private set; }
    }
}