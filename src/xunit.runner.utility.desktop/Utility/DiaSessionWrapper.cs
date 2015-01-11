using System;

namespace Xunit
{
    // This class wraps DiaSession, and uses DiaSessionWrapperHelper in the testing app domain to help us
    // discover when a test is an async test (since that requires special handling by DIA).
    internal class DiaSessionWrapper : IDisposable
    {
        readonly RemoteAppDomainManager appDomainManager;
        readonly DiaSessionWrapperHelper helper;
        readonly DiaSession session;

        public DiaSessionWrapper(string assemblyFilename)
        {
            session = new DiaSession(assemblyFilename);

            var assemblyFileName = typeof(DiaSessionWrapperHelper).Assembly.GetLocalCodeBase();

            appDomainManager = new RemoteAppDomainManager(assemblyFileName, null, true, null);
            helper = appDomainManager.CreateObject<DiaSessionWrapperHelper>(typeof(DiaSessionWrapperHelper).Assembly.FullName, typeof(DiaSessionWrapperHelper).FullName, assemblyFilename);
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