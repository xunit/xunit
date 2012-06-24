using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Xunit.Runner.VisualStudio
{
    // This class wraps DiaWrapper to get a delayed creation, as well as setting up the ability for
    // us to stop making queries once we know that DiaSession is going to throw (which probably means
    // that the PDB file is missing or corrupt).
    public class DiaSessionWrapper : IDisposable
    {
        string assemblyFilename;
        DiaSession session;
        bool sessionHasErrors;

        public DiaSessionWrapper(string assemblyFilename)
        {
            this.assemblyFilename = assemblyFilename;

            if (Environment.GetEnvironmentVariable("XUNIT_SKIP_DIA") != null)
                sessionHasErrors = true;
        }

        public DiaNavigationData GetNavigationData(string typeName, string methodName)
        {
            if (!sessionHasErrors)
                try
                {
                    if (session == null)
                    {
                        session = new DiaSession(assemblyFilename);
                    }

                    return session.GetNavigationData(typeName, methodName);
                }
                catch
                {
                    sessionHasErrors = true;
                }

            return null;
        }

        public void Dispose()
        {
            if (session != null)
                session.Dispose();
        }
    }
}
