using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyStarting"/>.
    /// </summary>
    public class TestAssemblyStarting : LongLivedMarshalByRefObject, ITestAssemblyStarting
    {
        public string AssemblyFileName { get; set; }

        public string ConfigFileName { get; set; }

        public DateTime StartTime { get; set; }

        public string TestEnvironment { get; set; }

        public string TestFrameworkDisplayName { get; set; }
    }
}