using System;

namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that the execution process is about to start for 
    /// the requested assembly.
    /// </summary>
    public interface ITestAssemblyStarting : ITestAssemblyMessage, IExecutionMessage
    {
        /// <summary>
        /// Gets the local date and time when the test assembly execution began.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Gets a display string that describes the test execution environment.
        /// </summary>
        string TestEnvironment { get; }

        /// <summary>
        /// Gets a display string which describes the test framework and version number.
        /// </summary>
        string TestFrameworkDisplayName { get; }
    }
}
