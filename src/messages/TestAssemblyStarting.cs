using System;
using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyStarting"/>.
    /// </summary>
    public class TestAssemblyStarting : TestAssemblyMessage, ITestAssemblyStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyStarting"/> class.
        /// </summary>
        public TestAssemblyStarting(IEnumerable<ITestCase> testCases, ITestAssembly testAssembly, DateTime startTime, string testEnvironment, string testFrameworkDisplayName)
            : base(testCases, testAssembly)
        {
            StartTime = startTime;
            TestEnvironment = testEnvironment;
            TestFrameworkDisplayName = testFrameworkDisplayName;
        }

        /// <inheritdoc/>
        public DateTime StartTime { get; set; }

        /// <inheritdoc/>
        public string TestEnvironment { get; set; }

        /// <inheritdoc/>
        public string TestFrameworkDisplayName { get; set; }
    }
}
