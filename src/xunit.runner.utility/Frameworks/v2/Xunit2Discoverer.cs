using System.IO;
using System.Reflection;
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
        readonly RemoteAppDomainManager appDomain;
        readonly ITestFrameworkDiscoverer discoverer;
        readonly ITestFramework framework;

        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit2Discoverer"/> class. The location
        /// of xunit.execution.dll is implied based on the location of the test assembly.
        /// </summary>
        /// <param name="sourceInformationProvider">The source code information provider.</param>
        /// <param name="assemblyInfo">The assembly to use for discovery</param>
        public Xunit2Discoverer(ISourceInformationProvider sourceInformationProvider, IAssemblyInfo assemblyInfo)
            : this(sourceInformationProvider, assemblyInfo, null, GetXunitExecutionAssemblyPath(assemblyInfo), null, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit2Discoverer"/> class. This constructor
        /// is usually used by AST-based runners which may not have access to a test assembly on disk,
        /// but can point to xunit.execution.dll (by following the project reference in Visual Studio).
        /// </summary>
        /// <param name="sourceInformationProvider">The source code information provider.</param>
        /// <param name="assemblyInfo">The assembly to use for discovery</param>
        /// <param name="xunitExecutionAssemblyPath">The path on disk of xunit.execution.dll</param>
        public Xunit2Discoverer(ISourceInformationProvider sourceInformationProvider, IAssemblyInfo assemblyInfo, string xunitExecutionAssemblyPath)
            : this(sourceInformationProvider, assemblyInfo, null, xunitExecutionAssemblyPath, null, true) { }

        // Used by Xunit2 when initializing for both discovery and execution.
        internal Xunit2Discoverer(ISourceInformationProvider sourceInformationProvider, string assemblyFileName, string configFileName, bool shadowCopy)
            : this(sourceInformationProvider, null, assemblyFileName, GetXunitExecutionAssemblyPath(assemblyFileName), configFileName, shadowCopy) { }

        Xunit2Discoverer(ISourceInformationProvider sourceInformationProvider, IAssemblyInfo assemblyInfo, string assemblyFileName, string xunitExecutionAssemblyPath, string configFileName, bool shadowCopy)
        {
            Guard.ArgumentNotNull("assemblyInfo", (object)assemblyInfo ?? assemblyFileName);
#if !ANDROID
            Guard.ArgumentValid("xunitExecutionAssemblyPath", "File not found: " + xunitExecutionAssemblyPath, File.Exists(xunitExecutionAssemblyPath));
#endif
            appDomain = new RemoteAppDomainManager(assemblyFileName ?? xunitExecutionAssemblyPath, configFileName, shadowCopy);

#if !ANDROID
            var name = AssemblyName.GetAssemblyName(xunitExecutionAssemblyPath);
            var testFrameworkAssemblyName = name.FullName;
#else
            var name = Assembly.Load(xunitExecutionAssemblyPath);
            var testFrameworkAssemblyName = name.FullName;
#endif

            // If we didn't get an assemblyInfo object, we can leverage the reflection-based IAssemblyInfo wrapper
            if (assemblyInfo == null)
                assemblyInfo = appDomain.CreateObject<IAssemblyInfo>(testFrameworkAssemblyName, "Xunit.Sdk.ReflectionAssemblyInfo", assemblyFileName);

            framework = appDomain.CreateObject<ITestFramework>(testFrameworkAssemblyName, "Xunit.Sdk.TestFrameworkProxy", assemblyInfo, sourceInformationProvider);
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
        public string TargetFramework
        {
            get { return discoverer.TargetFramework; }
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
            appDomain.SafeDispose();
        }

        /// <summary>
        /// Starts the process of finding all xUnit.net v2 tests in an assembly.
        /// </summary>
        /// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        /// <param name="options">The options used by the test framework during discovery.</param>
        public void Find(bool includeSourceInformation, IMessageSink messageSink, XunitDiscoveryOptions options)
        {
            discoverer.Find(includeSourceInformation, messageSink, options);
        }

        /// <inheritdoc/>
        void ITestFrameworkDiscoverer.Find(bool includeSourceInformation, IMessageSink messageSink, ITestFrameworkOptions options)
        {
            discoverer.Find(includeSourceInformation, messageSink, options);
        }

        /// <summary>
        /// Starts the process of finding all xUnit.net v2 tests in a class.
        /// </summary>
        /// <param name="typeName">The fully qualified type name to find tests in.</param>
        /// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        /// <param name="options">The options used by the test framework during discovery.</param>
        public void Find(string typeName, bool includeSourceInformation, IMessageSink messageSink, XunitDiscoveryOptions options)
        {
            discoverer.Find(typeName, includeSourceInformation, messageSink, options);
        }

        /// <inheritdoc/>
        void ITestFrameworkDiscoverer.Find(string typeName, bool includeSourceInformation, IMessageSink messageSink, ITestFrameworkOptions options)
        {
            discoverer.Find(typeName, includeSourceInformation, messageSink, options);
        }

        static string GetXunitExecutionAssemblyPath(string assemblyFileName)
        {
            Guard.ArgumentNotNullOrEmpty("assemblyFileName", assemblyFileName);
#if !ANDROID
            Guard.ArgumentValid("assemblyFileName", "File not found: " + assemblyFileName, File.Exists(assemblyFileName));
#endif

            return Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.execution.dll");
        }

        static string GetXunitExecutionAssemblyPath(IAssemblyInfo assemblyInfo)
        {
            Guard.ArgumentNotNull("assemblyInfo", assemblyInfo);
            Guard.ArgumentNotNullOrEmpty("assemblyInfo.AssemblyPath", assemblyInfo.AssemblyPath);

            return Path.Combine(Path.GetDirectoryName(assemblyInfo.AssemblyPath), "xunit.execution.dll");
        }

        /// <inheritdoc/>
        public string Serialize(ITestCase testCase)
        {
            return discoverer.Serialize(testCase);
        }
    }
}