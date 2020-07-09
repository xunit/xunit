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
            Guard.ArgumentNotNull(nameof(testCases), testCases);
            Guard.ArgumentNotNull(nameof(testAssembly), testAssembly);
            Guard.ArgumentNotNull(nameof(testEnvironment), testEnvironment);
            Guard.ArgumentNotNull(nameof(testFrameworkDisplayName), testFrameworkDisplayName);

            StartTime = startTime;
            TestEnvironment = testEnvironment;
            TestFrameworkDisplayName = testFrameworkDisplayName;
        }

        /// <inheritdoc/>
        public DateTime StartTime { get; }

        /// <inheritdoc/>
        public string TestEnvironment { get; }

        /// <inheritdoc/>
        public string TestFrameworkDisplayName { get; }
    }
}
