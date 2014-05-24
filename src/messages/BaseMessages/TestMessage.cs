using System;
using System.Linq;
using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestMessage"/>.
    /// </summary>
    public class TestMessage : TestCaseMessage, ITestMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMessage"/> class.
        /// </summary>
        public TestMessage(ITestCase testCase, string testDisplayName)
            : base(testCase)
        {
            TestDisplayName = testDisplayName;
        }

        /// <inheritdoc/>
        public string TestDisplayName { get; private set; }
    }
}
