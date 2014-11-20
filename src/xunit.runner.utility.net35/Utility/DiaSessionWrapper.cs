using System;
using System.IO;
using System.Reflection;

namespace Xunit
{
    // This class wraps DiaSession, and uses DiaSessionWrapperHelper in the testing app domain to help us
    // discover when a test is an async test (since that requires special handling by DIA).
    internal class DiaSessionWrapper : IDisposable
    {
        readonly RemoteAppDomainManager appDomainManager;
        readonly DiaSessionWrapperHelper helper;
        readonly DiaSession session;

        public DiaSessionWrapper(string assemblyFilename, bool shadowCopy = true, string configFileName = null)
        {
            session = new DiaSession(assemblyFilename);

            string xUnitAssemblyPath = typeof(DiaSessionWrapperHelper).Assembly.GetLocalCodeBase();
            string xUnitAssemblyDirectory = Path.GetDirectoryName(xUnitAssemblyPath);

            appDomainManager = new RemoteAppDomainManager(assemblyFilename, configFileName, shadowCopy, null);

            // We want to be able to create the wrapper in the other domain, yet keep the domain centered on the test assembly.
            helper = appDomainManager.CreateObjectFromPath<DiaSessionWrapperHelper>(xUnitAssemblyPath, typeof(DiaSessionWrapperHelper).FullName, assemblyFilename, xUnitAssemblyDirectory);
        }

        public DiaNavigationData GetNavigationData(string typeName, string methodName)
        {
            helper.Normalize(ref typeName, ref methodName);
            return session.GetNavigationData(typeName, methodName);
        }

        public void Dispose()
        {
            session.Dispose();
            appDomainManager.Dispose();
        }
    }
}