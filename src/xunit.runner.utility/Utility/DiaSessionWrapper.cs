using System;
using System.IO;
using System.Security;
using System.Security.Permissions;

namespace Xunit
{
    // This class wraps DiaSession, and uses DiaSessionWrapperHelper in the testing app domain to help us
    // discover when a test is an async test (since that requires special handling by DIA).
    internal class DiaSessionWrapper : IDisposable
    {
        readonly AppDomain appDomain;
        readonly DiaSessionWrapperHelper helper;
        readonly DiaSession session;

        public DiaSessionWrapper(string assemblyFilename)
        {
            session = new DiaSession(assemblyFilename);

            var setup = new AppDomainSetup
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

        public DiaNavigationData GetNavigationData(string typeName, string methodName)
        {
            helper.Normalize(ref typeName, ref methodName);
            return session.GetNavigationData(typeName, methodName);
        }

        public void Dispose()
        {
            helper.SafeDispose();
            session.SafeDispose();

            if (appDomain != null)
                AppDomain.Unload(appDomain);
        }
    }
}