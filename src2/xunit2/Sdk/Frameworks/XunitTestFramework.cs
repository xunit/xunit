using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestFramework"/> that supports discovery and
    /// execution of unit tests linked against xunit2.dll.
    /// </summary>
    public class XunitTestFramework : LongLivedMarshalByRefObject, ITestFramework
    {
        /// <inheritdoc/>
        public ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assemblyInfo)
        {
            return new XunitTestFrameworkDiscoverer(assemblyInfo);
        }

        /// <inheritdoc/>
        public ITestFrameworkExecutor GetExecutor(string assemblyFileName)
        {
            return new XunitTestFrameworkExecutor(assemblyFileName);
        }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}