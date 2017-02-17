#if !PLATFORM_DOTNET

using System;

namespace Xunit
{
    // This class wraps DiaSession, and uses DiaSessionWrapperHelper in the testing app domain to help us
    // discover when a test is an async test (since that requires special handling by DIA).
    class DiaSessionWrapper : IDisposable
    {
        readonly DiaSession session;

        public DiaSessionWrapper(string assemblyFilename)
        {
            session = new DiaSession(assemblyFilename);
        }

        public DiaNavigationData GetNavigationData(string typeName, string methodName)
        {
            return session.GetNavigationData(typeName, methodName, session.AssemblyFileName);
        }

        public void Dispose()
        {
            session.Dispose();
        }
    }
}
#endif