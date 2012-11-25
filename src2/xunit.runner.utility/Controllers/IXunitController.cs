using System;
using Xunit.Abstractions;

namespace Xunit
{
    public interface IXunitController : ITestFramework, IDisposable
    {
        /// <summary>
        /// Gets the full pathname to the assembly under test.
        /// </summary>
        string AssemblyFileName { get; }

        /// <summary>
        /// Gets the full pathname to the configuration file.
        /// </summary>
        string ConfigFileName { get; }

        /// <summary>
        /// Gets the version of xunit.dll used by the test assembly.
        /// </summary>
        string XunitVersion { get; }
    }
}
