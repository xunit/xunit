using System;
using System.Reflection;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a test framework. There are two pieces to test frameworks: discovery and
    /// execution. The two factory methods represent these two pieces. Test frameworks can
    /// implement an empty constructor, or they can implement one that takes <see cref="Xunit.Abstractions.IMessageSink"/>
    /// if they want to be able to send diagnostic messages.
    /// </summary>
    public interface ITestFramework : IDisposable
    {
        /// <summary>
        /// Sets the source information provider to be used during discovery.
        /// </summary>
        ISourceInformationProvider SourceInformationProvider { set; }

        /// <summary>
        /// Get a test discoverer.
        /// </summary>
        /// <param name="assembly">The assembly from which to discover the tests.</param>
        /// <returns>The test discoverer.</returns>
        ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assembly);

        /// <summary>
        /// Get a test executor.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to run tests from.</param>
        /// <returns>The test executor.</returns>
        ITestFrameworkExecutor GetExecutor(AssemblyName assemblyName);
    }
}