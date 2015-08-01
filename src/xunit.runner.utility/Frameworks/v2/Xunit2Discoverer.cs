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
        readonly IAppDomainManager appDomain;
        readonly ITestFrameworkDiscoverer discoverer;
        readonly ITestFramework framework;

        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit2Discoverer"/> class.
        /// </summary>
        /// <param name="useAppDomain">Determines whether tests should be run in a separate app domain.</param>
        /// <param name="sourceInformationProvider">The source code information provider.</param>
        /// <param name="assemblyInfo">The assembly to use for discovery</param>
        /// <param name="xunitExecutionAssemblyPath">The path on disk of xunit.execution.dll; if <c>null</c>, then
        /// the location of xunit.execution.dll is implied based on the location of the test assembly</param>
        /// <param name="shadowCopyFolder">The path on disk to use for shadow copying; if <c>null</c>, a folder
        /// will be automatically (randomly) generated</param>
        /// <param name="diagnosticMessageSink">The message sink which received <see cref="IDiagnosticMessage"/> messages.</param>
        /// <param name="verifyAssembliesOnDisk">Determines whether or not to check for the existence of assembly files.</param>
        public Xunit2Discoverer(bool useAppDomain,
                                ISourceInformationProvider sourceInformationProvider,
                                IAssemblyInfo assemblyInfo,
                                string xunitExecutionAssemblyPath = null,
                                string shadowCopyFolder = null,
                                IMessageSink diagnosticMessageSink = null,
                                bool verifyAssembliesOnDisk = true)
            : this(useAppDomain, sourceInformationProvider, assemblyInfo, null, xunitExecutionAssemblyPath ?? GetXunitExecutionAssemblyPath(assemblyInfo), null, true, shadowCopyFolder, diagnosticMessageSink, verifyAssembliesOnDisk)
        { }

        // Used by Xunit2 when initializing for both discovery and execution.
        internal Xunit2Discoverer(bool useAppDomain,
                                  ISourceInformationProvider sourceInformationProvider,
                                  string assemblyFileName,
                                  string configFileName,
                                  bool shadowCopy,
                                  string shadowCopyFolder = null,
                                  IMessageSink diagnosticMessageSink = null,
                                  bool verifyAssembliesOnDisk = true)
            : this(useAppDomain, sourceInformationProvider, null, assemblyFileName, GetXunitExecutionAssemblyPath(assemblyFileName, verifyAssembliesOnDisk), configFileName, shadowCopy, shadowCopyFolder, diagnosticMessageSink, verifyAssembliesOnDisk)
        { }

        Xunit2Discoverer(bool useAppDomain,
                         ISourceInformationProvider sourceInformationProvider,
                         IAssemblyInfo assemblyInfo,
                         string assemblyFileName,
                         string xunitExecutionAssemblyPath,
                         string configFileName,
                         bool shadowCopy,
                         string shadowCopyFolder,
                         IMessageSink diagnosticMessageSink,
                         bool verifyAssembliesOnDisk)
        {
            Guard.ArgumentNotNull("assemblyInfo", (object)assemblyInfo ?? assemblyFileName);
            if (verifyAssembliesOnDisk)
                Guard.FileExists("xunitExecutionAssemblyPath", xunitExecutionAssemblyPath);

            DiagnosticMessageSink = diagnosticMessageSink ?? new NullMessageSink();

            var appDomainAssembly = assemblyFileName ?? xunitExecutionAssemblyPath;
            appDomain = AppDomainManagerFactory.Create(useAppDomain, appDomainAssembly, configFileName, shadowCopy, shadowCopyFolder);

            var testFrameworkAssemblyName = GetTestFrameworkAssemblyName(xunitExecutionAssemblyPath);

            // If we didn't get an assemblyInfo object, we can leverage the reflection-based IAssemblyInfo wrapper
            if (assemblyInfo == null)
                assemblyInfo = appDomain.CreateObject<IAssemblyInfo>(testFrameworkAssemblyName, "Xunit.Sdk.ReflectionAssemblyInfo", assemblyFileName);

            framework = appDomain.CreateObject<ITestFramework>(testFrameworkAssemblyName, "Xunit.Sdk.TestFrameworkProxy", assemblyInfo, sourceInformationProvider, DiagnosticMessageSink);
            discoverer = Framework.GetDiscoverer(assemblyInfo);
        }

        /// <summary>
        /// Gets the message sink used to report diagnostic messages.
        /// </summary>
        public IMessageSink DiagnosticMessageSink { get; private set; }

        static AssemblyName GetTestFrameworkAssemblyName(string xunitExecutionAssemblyPath)
        {
#if ANDROID
            // Android needs to just load the assembly
            return Assembly.Load(xunitExecutionAssemblyPath).GetName();
#elif WINDOWS_PHONE_APP || WINDOWS_PHONE || DOTNETCORE
            // Make sure we only use the short form
            return Assembly.Load(new AssemblyName { Name = Path.GetFileNameWithoutExtension(xunitExecutionAssemblyPath), Version = new System.Version(0, 0, 0, 0) }).GetName();
#else
            return AssemblyName.GetAssemblyName(xunitExecutionAssemblyPath);
#endif
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
        /// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
        public void Find(bool includeSourceInformation, IMessageSink messageSink, ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            discoverer.Find(includeSourceInformation, messageSink, discoveryOptions);
        }

        /// <summary>
        /// Starts the process of finding all xUnit.net v2 tests in a class.
        /// </summary>
        /// <param name="typeName">The fully qualified type name to find tests in.</param>
        /// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        /// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
        public void Find(string typeName, bool includeSourceInformation, IMessageSink messageSink, ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            discoverer.Find(typeName, includeSourceInformation, messageSink, discoveryOptions);
        }

        static string GetXunitExecutionAssemblyPath(string assemblyFileName, bool verifyTestAssemblyExists)
        {
            Guard.ArgumentNotNullOrEmpty("assemblyFileName", assemblyFileName);
            if (verifyTestAssemblyExists)
                Guard.FileExists("assemblyFileName", assemblyFileName);

            return Path.Combine(Path.GetDirectoryName(assemblyFileName), ExecutionHelper.AssemblyFileName);
        }

        static string GetXunitExecutionAssemblyPath(IAssemblyInfo assemblyInfo)
        {
            Guard.ArgumentNotNull("assemblyInfo", assemblyInfo);
            Guard.ArgumentNotNullOrEmpty("assemblyInfo.AssemblyPath", assemblyInfo.AssemblyPath);

            return Path.Combine(Path.GetDirectoryName(assemblyInfo.AssemblyPath), ExecutionHelper.AssemblyFileName);
        }

        /// <inheritdoc/>
        public string Serialize(ITestCase testCase)
        {
            return discoverer.Serialize(testCase);
        }
    }
}
