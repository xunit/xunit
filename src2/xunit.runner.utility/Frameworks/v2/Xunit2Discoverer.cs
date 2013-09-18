using System.IO;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// This class be used to do discovery of xUnit.net v2 tests, via any implementation
    /// of <see cref="IAssemblyInfo"/>, including AST-based runners like CodeRush and
    /// Resharper. Runner authors who are not using AST-based discovery are strongly
    /// encouraged to use <see cref="XunitFrontController"/> instead.
    /// </summary>
    public class Xunit2Discoverer : ITestFrameworkDiscoverer
    {
        readonly ITestFrameworkDiscoverer discoverer;
        readonly AppDomainTestFramework framework;

        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit2Discoverer"/> class. The location
        /// of xunit2.dll is implied based on the location of the test assembly.
        /// </summary>
        /// <param name="sourceInformationProvider">The source code information provider.</param>
        /// <param name="assemblyInfo">The assembly to use for discovery</param>
        public Xunit2Discoverer(ISourceInformationProvider sourceInformationProvider, IAssemblyInfo assemblyInfo)
            : this(sourceInformationProvider, assemblyInfo, null, GetXunit2AssemblyPath(assemblyInfo), null, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit2Discoverer"/> class. This constructor
        /// is usually used by AST-based runners which may not have access to a test assembly on disk,
        /// but can point to xunit2.dll (by following the project reference in Visual Studio).
        /// </summary>
        /// <param name="sourceInformationProvider">The source code information provider.</param>
        /// <param name="assemblyInfo">The assembly to use for discovery</param>
        /// <param name="xunit2AssemblyPath">The path on disk of xunit2.dll</param>
        public Xunit2Discoverer(ISourceInformationProvider sourceInformationProvider, IAssemblyInfo assemblyInfo, string xunit2AssemblyPath)
            : this(sourceInformationProvider, assemblyInfo, null, xunit2AssemblyPath, null, true) { }

        // Used by Xunit2 when initializing for both discovery and execution.
        internal Xunit2Discoverer(ISourceInformationProvider sourceInformationProvider, string assemblyFileName, string configFileName, bool shadowCopy)
            : this(sourceInformationProvider, null, assemblyFileName, GetXunit2AssemblyPath(assemblyFileName), configFileName, shadowCopy) { }

        Xunit2Discoverer(ISourceInformationProvider sourceInformationProvider, IAssemblyInfo assemblyInfo, string assemblyFileName, string xunit2AssemblyPath, string configFileName, bool shadowCopy)
        {
            Guard.ArgumentNotNull("assemblyInfo", (object)assemblyInfo ?? assemblyFileName);
            Guard.ArgumentValid("xunit2AssemblyPath", "File not found: " + xunit2AssemblyPath, File.Exists(xunit2AssemblyPath));

            framework = new AppDomainTestFramework(sourceInformationProvider, assemblyFileName, xunit2AssemblyPath, "Xunit.Sdk.XunitTestFramework", configFileName, shadowCopy);

            // If we didn't get an assemblyInfo object, we can leverage the reflection-based IAssemblyInfo wrapper
            if (assemblyInfo == null)
                assemblyInfo = framework.CreateRemoteObject<IAssemblyInfo>("Xunit.Sdk.ReflectionAssemblyInfo", assemblyFileName);

            discoverer = Framework.GetDiscoverer(assemblyInfo);
        }

        /// <summary>
        /// Returns the test framework from the remote app domain.
        /// </summary>
        public ITestFramework Framework
        {
            get { return framework; }
        }

        /// <inheritdoc/>
        public string TestFrameworkDisplayName
        {
            get { return discoverer.TestFrameworkDisplayName; }
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            discoverer.SafeDispose();
            Framework.SafeDispose();
        }

        /// <inheritdoc/>
        public void Find(bool includeSourceInformation, IMessageSink messageSink)
        {
            discoverer.Find(includeSourceInformation, messageSink);
        }

        /// <inheritdoc/>
        public void Find(string typeName, bool includeSourceInformation, IMessageSink messageSink)
        {
            discoverer.Find(typeName, includeSourceInformation, messageSink);
        }

        static string GetXunit2AssemblyPath(string assemblyFileName)
        {
            Guard.ArgumentNotNullOrEmpty("assemblyFileName", assemblyFileName);
            Guard.ArgumentValid("assemblyFileName", "File not found: " + assemblyFileName, File.Exists(assemblyFileName));

            return Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit2.dll");
        }

        static string GetXunit2AssemblyPath(IAssemblyInfo assemblyInfo)
        {
            Guard.ArgumentNotNull("assemblyInfo", assemblyInfo);
            Guard.ArgumentNotNullOrEmpty("assemblyInfo.AssemblyPath", assemblyInfo.AssemblyPath);

            return Path.Combine(Path.GetDirectoryName(assemblyInfo.AssemblyPath), "xunit2.dll");
        }

        /// <inheritdoc/>
        public string Serialize(ITestCase testCase)
        {
            return discoverer.Serialize(testCase);
        }
    }
}