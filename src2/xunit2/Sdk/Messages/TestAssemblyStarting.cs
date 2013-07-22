using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyStarting"/>.
    /// </summary>
    public class TestAssemblyStarting : LongLivedMarshalByRefObject, ITestAssemblyStarting
    {
        /// <inheritdoc/>
        public string AssemblyFileName { get; set; }

        /// <inheritdoc/>
        public string ConfigFileName { get; set; }

        /// <inheritdoc/>
        public DateTime StartTime { get; set; }

        /// <inheritdoc/>
        public string TestEnvironment { get; set; }

        /// <inheritdoc/>
        public string TestFrameworkDisplayName { get; set; }
    }
}