using System;

namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that the execution process is about to start for 
    /// the requested assembly.
    /// </summary>
    public interface ITestAssemblyStarting : ITestMessage
    {
        string AssemblyFileName { get; }

        string ConfigFileName { get; }

        DateTime StartTime { get; }

        string TestEnvironment { get; }

        string TestFrameworkDisplayName { get; }
    }
}
