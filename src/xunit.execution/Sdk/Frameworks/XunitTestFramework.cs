using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestFramework"/> that supports discovery and
    /// execution of unit tests linked against xunit.core.dll, using xunit.execution.dll.
    /// </summary>
    public class XunitTestFramework : TestFramework
    {
        /// <inheritdoc/>
        protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo)
        {
            return new XunitTestFrameworkDiscoverer(assemblyInfo, SourceInformationProvider);
        }

        /// <inheritdoc/>
        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            return new XunitTestFrameworkExecutor(assemblyName, SourceInformationProvider);
        }
    }
}