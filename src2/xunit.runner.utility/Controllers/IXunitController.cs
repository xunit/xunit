using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    public interface IXunitController : IDisposable
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

        /// <summary>
        /// Enumerates the tests in an assembly.
        /// </summary>
        /// <returns>The list of test cases in the test assembly.</returns>
        IEnumerable<ITestCase> EnumerateTests();
    }
}
