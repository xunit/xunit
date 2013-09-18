using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyStarting"/>.
    /// </summary>
    internal class TestAssemblyStarting : LongLivedMarshalByRefObject, ITestAssemblyStarting
    {
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