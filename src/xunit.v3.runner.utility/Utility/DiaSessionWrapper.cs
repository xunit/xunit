#if NETFRAMEWORK

using System;

namespace Xunit
{
    // This class wraps DiaSession, and uses DiaSessionWrapperHelper in the testing app domain to help us
    // discover when a test is an async test (since that requires special handling by DIA).
    class DiaSessionWrapper : IDisposable
    {
        readonly AppDomainManager_AppDomain appDomainManager;
        readonly DiaSessionWrapperHelper helper;
        readonly DiaSession session;

        public DiaSessionWrapper(string assemblyFilename)
        {
            session = new DiaSession(assemblyFilename);

            var assemblyFileName = typeof(DiaSessionWrapperHelper).Assembly.GetLocalCodeBase();

            appDomainManager = new AppDomainManager_AppDomain(assemblyFileName, null, true, null);
            helper = appDomainManager.CreateObject<DiaSessionWrapperHelper>(typeof(DiaSessionWrapperHelper).Assembly.GetName(), typeof(DiaSessionWrapperHelper).FullName, assemblyFilename);
        }

        public DiaNavigationData GetNavigationData(string typeName, string methodName)
        {
            var owningAssemblyFilename = session.AssemblyFileName;
            helper.Normalize(ref typeName, ref methodName, ref owningAssemblyFilename);
            return session.GetNavigationData(typeName, methodName, owningAssemblyFilename);
        }

        public void Dispose()
        {
            session.Dispose();
            appDomainManager.Dispose();
        }
    }
}
#endif
