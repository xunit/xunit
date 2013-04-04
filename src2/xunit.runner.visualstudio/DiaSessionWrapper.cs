using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Xunit.Runner.VisualStudio
{
    // This class wraps DiaWrapper to get a delayed creation, as well as setting up the ability for
    // us to stop making queries once we know that DiaSession is going to throw (which probably means
    // that the PDB file is missing or corrupt).
    public class DiaSessionWrapper : IDisposable
    {
        AppDomain appDomain;
        string assemblyFilename;
        DiaSessionWrapperHelper helper;
        DiaSession session;
        bool sessionHasErrors;

        public DiaSessionWrapper(string assemblyFilename)
        {
            this.assemblyFilename = assemblyFilename;

            if (Environment.GetEnvironmentVariable("XUNIT_SKIP_DIA") != null)
                sessionHasErrors = true;
            else
            {
                try
                {
                    AppDomainSetup setup = new AppDomainSetup
                    {
                        ApplicationBase = Path.GetDirectoryName(new Uri(typeof(DiaSessionWrapperHelper).Assembly.CodeBase).LocalPath),
                        ApplicationName = Guid.NewGuid().ToString(),
                        LoaderOptimization = LoaderOptimization.MultiDomainHost,
                        ShadowCopyFiles = "true",
                    };

                    setup.ShadowCopyDirectories = setup.ApplicationBase;
                    setup.CachePath = Path.Combine(Path.GetTempPath(), setup.ApplicationName);

                    appDomain = AppDomain.CreateDomain(setup.ApplicationName, null, setup, new PermissionSet(PermissionState.Unrestricted));

                    helper = (DiaSessionWrapperHelper)appDomain.CreateInstanceAndUnwrap(
                        assemblyName: typeof(DiaSessionWrapperHelper).Assembly.FullName,
                        typeName: typeof(DiaSessionWrapperHelper).FullName,
                        ignoreCase: false,
                        bindingAttr: 0,
                        binder: null,
                        args: new[] { assemblyFilename },
                        culture: null,
                        activationAttributes: null,
                        securityAttributes: null
                    );
                }
                catch
                {
                    sessionHasErrors = true;
                }
            }
        }

        private MethodInfo GetMethod(string typeName, string methodName)
        {
            return null;
        }

        public DiaNavigationData GetNavigationData(string typeName, string methodName)
        {
            if (!sessionHasErrors)
                try
                {
                    helper.Normalize(ref typeName, ref methodName);

                    if (session == null)
                        session = new DiaSession(assemblyFilename);

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

            if (appDomain != null)
                AppDomain.Unload(appDomain);
        }
    }
}