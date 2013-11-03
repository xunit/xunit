using System.IO;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Wraps around an implementation of <see cref="ITestFramework"/> that is run from another
    /// app domain. This is typically done to ensure that assemblies aren't locked during discovery
    /// and run, so that further builds can take place while activities are still ongoing.
    /// </summary>
    public class AppDomainTestFramework : ITestFramework
    {
        readonly RemoteAppDomainManager appDomain;
        readonly ITestFramework testFramework;
        readonly AssemblyName testFrameworkAssemblyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDomainTestFramework"/> class.
        /// </summary>
        /// <param name="sourceInformationProvider">The source code information provider.</param>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="testFrameworkFileName">The file path of the test framework assembly (i.e., xunit2.dll).</param>
        /// <param name="testFrameworkTypeName">The fully qualified type name of the implementation of <see cref="ITestFramework"/>
        /// in the test framework assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <param name="shadowCopy">If set to <c>true</c>, runs tests in a shadow copied app domain, which allows
        /// tests to be discovered and run without locking assembly files on disk.</param>
        public AppDomainTestFramework(ISourceInformationProvider sourceInformationProvider, string assemblyFileName, string testFrameworkFileName, string testFrameworkTypeName, string configFileName = null, bool shadowCopy = true)
        {
            Guard.ArgumentNotNull("sourceInformationProvider", sourceInformationProvider);
            Guard.ArgumentNotNullOrEmpty("testFrameworkFileName", testFrameworkFileName);

            testFrameworkFileName = Path.GetFullPath(testFrameworkFileName);
            Guard.ArgumentValid("testFrameworkFileName", "File not found: " + testFrameworkFileName, File.Exists(testFrameworkFileName));

            SourceInformationProvider = sourceInformationProvider;

            // assemblyFileName might be null (during AST-based discovery), so pass along with the test
            // framework filename instead if we don't have an assembly under test yet.
            appDomain = new RemoteAppDomainManager(assemblyFileName ?? testFrameworkFileName, configFileName, shadowCopy);

            testFrameworkAssemblyName = AssemblyName.GetAssemblyName(testFrameworkFileName);
            testFramework = appDomain.CreateObject<ITestFramework>(testFrameworkAssemblyName.FullName, testFrameworkTypeName);
            testFramework.SourceInformationProvider = SourceInformationProvider;
        }

        /// <inheritdoc/>
        public ISourceInformationProvider SourceInformationProvider { get; set; }

        /// <summary>
        /// Creates an object (from the test framework assembly) in the remote app domain.
        /// </summary>
        /// <typeparam name="T">The type of the object to cast to.</typeparam>
        /// <param name="typeName">The fully qualified type name to create.</param>
        /// <param name="args">The arguments for the type's constructor.</param>
        /// <returns>An instance of the created object, cast to <typeparamref name="T"/>.</returns>
        public T CreateRemoteObject<T>(string typeName, params object[] args)
        {
            return appDomain.CreateObject<T>(testFrameworkAssemblyName.FullName, typeName, args);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (testFramework != null)
                testFramework.Dispose();

            if (appDomain != null)
                appDomain.Dispose();
        }

        /// <inheritdoc/>
        public ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assembly)
        {
            return testFramework.GetDiscoverer(assembly);
        }

        /// <inheritdoc/>
        public ITestFrameworkExecutor GetExecutor(string assemblyFileName)
        {
            return testFramework.GetExecutor(assemblyFileName);
        }
    }
}