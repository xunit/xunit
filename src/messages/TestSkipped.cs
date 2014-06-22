﻿using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestSkipped"/>.
    /// </summary>
    public class TestSkipped : TestResultMessage, ITestSkipped
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestSkipped"/> class.
        /// </summary>
        public TestSkipped(ITestCase testCase, string testDisplayName, string reason)
            : base(testCase, testDisplayName, 0, null)
        {
            Reason = reason;
        }

        /// <inheritdoc/>
        public string Reason { get; private set; }
    }
}