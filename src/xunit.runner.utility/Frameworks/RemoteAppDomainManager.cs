using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Xunit
{
    internal class RemoteAppDomainManager : IDisposable
    {
        public RemoteAppDomainManager(string assemblyFileName, string configFileName, bool shadowCopy)
        {
            Guard.ArgumentNotNullOrEmpty("assemblyFileName", assemblyFileName);

            assemblyFileName = Path.GetFullPath(assemblyFileName);
            Guard.ArgumentValid("assemblyFileName", "Could not find file: " + assemblyFileName, File.Exists(assemblyFileName));

            if (configFileName == null)
                configFileName = GetDefaultConfigFile(assemblyFileName);

            if (configFileName != null)
                configFileName = Path.GetFullPath(configFileName);

            AssemblyFileName = assemblyFileName;
            ConfigFileName = configFileName;
            AppDomain = CreateAppDomain(assemblyFileName, configFileName, shadowCopy);
        }

        public AppDomain AppDomain { get; private set; }

        public string AssemblyFileName { get; private set; }

        public string ConfigFileName { get; private set; }

        static AppDomain CreateAppDomain(string assemblyFilename, string configFilename, bool shadowCopy)
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(assemblyFilename);
            setup.ApplicationName = Guid.NewGuid().ToString();

            if (shadowCopy)
            {
                setup.ShadowCopyFiles = "true";
                setup.ShadowCopyDirectories = setup.ApplicationBase;
                setup.CachePath = Path.Combine(Path.GetTempPath(), setup.ApplicationName);
            }

            setup.ConfigurationFile = configFilename;

            return AppDomain.CreateDomain(setup.ApplicationName, null, setup, new PermissionSet(PermissionState.Unrestricted));
        }

        public TObject CreateObject<TObject>(string assemblyName, string typeName, params object[] args)
        {
            try
            {
                object unwrappedObject = AppDomain.CreateInstanceAndUnwrap(assemblyName, typeName, false, 0, null, args, null, null, null);
                return (TObject)unwrappedObject;
            }
            catch (TargetInvocationException ex)
            {
                ex.InnerException.RethrowWithNoStackTraceLoss();
                return default(TObject);
            }
        }

        public virtual void Dispose()
        {
            if (AppDomain != null)
            {
                string cachePath = AppDomain.SetupInformation.CachePath;

                //System.AppDomain.Unload(AppDomain);

                try
                {
                    if (cachePath != null)
                        Directory.Delete(cachePath, true);
                }
                catch { }
            }
        }

        static string GetDefaultConfigFile(string assemblyFile)
        {
            string configFilename = assemblyFile + ".config";

            if (File.Exists(configFilename))
                return configFilename;

            return null;
        }
    }
}