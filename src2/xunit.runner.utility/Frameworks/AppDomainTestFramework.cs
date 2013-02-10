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

        public AppDomainTestFramework(string assemblyFileName, string testFrameworkFileName, string testFrameworkTypeName, string configFileName = null, bool shadowCopy = true)
        {
            Guard.ArgumentNotNullOrEmpty("testFrameworkFileName", testFrameworkFileName);

            testFrameworkFileName = Path.GetFullPath(testFrameworkFileName);
            Guard.ArgumentValid("testFrameworkFileName", "File not found: " + testFrameworkFileName, File.Exists(testFrameworkFileName));

            // assemblyFileName might be null (during AST-based discovery), so pass along with the test
            // framework filename instead if we don't have an assembly under test yet.
            appDomain = new RemoteAppDomainManager(assemblyFileName ?? testFrameworkFileName, configFileName, shadowCopy);

            testFrameworkAssemblyName = AssemblyName.GetAssemblyName(testFrameworkFileName);
            testFramework = appDomain.CreateObject<ITestFramework>(testFrameworkAssemblyName.FullName, "Xunit.Sdk.XunitTestFramework");
        }

        public T CreateRemoteObject<T>(string typeName, params object[] args)
        {
            return appDomain.CreateObject<T>(testFrameworkAssemblyName.FullName, typeName, args);
        }

        public void Dispose()
        {
            if (testFramework != null)
                testFramework.Dispose();

            if (appDomain != null)
                appDomain.Dispose();
        }

        public ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assembly)
        {
            return testFramework.GetDiscoverer(assembly);
        }

        public ITestFrameworkExecutor GetExecutor(string assemblyFileName)
        {
            return testFramework.GetExecutor(assemblyFileName);
        }
    }
}