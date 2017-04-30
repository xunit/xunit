#if !NETSTANDARD1_1

using System;

namespace Xunit
{
    // This class wraps DiaSession, and uses DiaSessionWrapperHelper to help us
    // discover when a test is an async test (since that requires special handling by DIA).
    class DiaSessionWrapper : IDisposable
    {
        readonly DiaSession session;

#if NETSTANDARD1_5 || NETCOREAPP1_0
        readonly DiaSessionWrapperHelper helper;

        public DiaSessionWrapper(string assemblyFilename)
        {
            session = new DiaSession(assemblyFilename);

            helper = new DiaSessionWrapperHelper(assemblyFilename);
        }

        public DiaNavigationData GetNavigationData(string typeName, string methodName)
        {
            var owningAssemblyFilename = session.AssemblyFileName;
            helper.Normalize(ref typeName, ref methodName, ref owningAssemblyFilename);
            return session.GetNavigationData(typeName, methodName, owningAssemblyFilename);
        }
#else
        public DiaSessionWrapper(string assemblyFilename)
            => session = new DiaSession(assemblyFilename);

        public DiaNavigationData GetNavigationData(string typeName, string methodName, string owningAssemblyFileName)
            => session.GetNavigationData(typeName, methodName, owningAssemblyFileName);
#endif

        public void Dispose()
            => session.Dispose();
    }
}

#endif
