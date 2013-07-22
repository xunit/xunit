using System;

namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that the execution process is about to start for 
    /// the requested assembly.
    /// </summary>
    public interface ITestAssemblyStarting : ITestMessage
    {
        /// <summary>
        /// Gets the full path of the test assembly file name.
        /// </summary>
        string AssemblyFileName { get; }

        /// <summary>
        /// Gets the full path of the configuraiton file name, if one is present.
        /// May be <c>null</c> if there is no configuration file.
        /// </summary>
        string ConfigFileName { get; }

        /// <summary>
        /// Gets the local date and time when the test assembly execution began.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Gets a display string that describes the test execution environment.
        /// </summary>
        string TestEnvironment { get; }

        /// <summary>
        /// Gets a display string which descibes the test framework and version number.
        /// </summary>
        string TestFrameworkDisplayName { get; }
    }
}
