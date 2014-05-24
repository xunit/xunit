using System;
using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyStarting"/>.
    /// </summary>
    public class TestAssemblyStarting : LongLivedMarshalByRefObject, ITestAssemblyStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyStarting"/> class.
        /// </summary>
        public TestAssemblyStarting(string assemblyFileName, string configFileName, DateTime startTime, string testEnvironment, string testFrameworkDisplayName)
        {
            AssemblyFileName = assemblyFileName;
            ConfigFileName = configFileName;
            StartTime = startTime;
            TestEnvironment = testEnvironment;
            TestFrameworkDisplayName = testFrameworkDisplayName;
        }

        /// <inheritdoc/>
        public string AssemblyFileName { get; private set; }

        /// <inheritdoc/>
        public string ConfigFileName { get; private set; }

        /// <inheritdoc/>
        public DateTime StartTime { get; private set; }

        /// <inheritdoc/>
        public string TestEnvironment { get; private set; }

        /// <inheritdoc/>
        public string TestFrameworkDisplayName { get; private set; }
    }
}