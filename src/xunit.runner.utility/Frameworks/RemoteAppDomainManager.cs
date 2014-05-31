using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Xunit
{
    internal class RemoteAppDomainManager : IDisposable
    {
        public RemoteAppDomainManager(string assemblyFileName, string configFileName, bool shadowCopy, string shadowCopyFolder)
        {
            Guard.ArgumentNotNullOrEmpty("assemblyFileName", assemblyFileName);

            assemblyFileName = Path.GetFullPath(assemblyFileName);
#if !ANDROID
            Guard.ArgumentValid("assemblyFileName", "Could not find file: " + assemblyFileName, File.Exists(assemblyFileName));
#endif
            if (configFileName == null)
                configFileName = GetDefaultConfigFile(assemblyFileName);

            if (configFileName != null)
                configFileName = Path.GetFullPath(configFileName);

            AssemblyFileName = assemblyFileName;
            ConfigFileName = configFileName;
#if !NO_APPDOMAIN
            AppDomain = CreateAppDomain(assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);
#endif
        }

        public AppDomain AppDomain { get; private set; }

        public string AssemblyFileName { get; private set; }

        public string ConfigFileName { get; private set; }

#if !NO_APPDOMAIN
        static AppDomain CreateAppDomain(string assemblyFilename, string configFilename, bool shadowCopy, string shadowCopyFolder)
        {
            var setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(assemblyFilename);
            setup.ApplicationName = Guid.NewGuid().ToString();

            if (shadowCopy)
            {
                setup.ShadowCopyFiles = "true";
                setup.ShadowCopyDirectories = setup.ApplicationBase;
                setup.CachePath = shadowCopyFolder ?? Path.Combine(Path.GetTempPath(), setup.ApplicationName);
            }

            setup.ConfigurationFile = configFilename;

            return AppDomain.CreateDomain(setup.ApplicationName, null, setup, new PermissionSet(PermissionState.Unrestricted));

        }
#endif

        public TObject CreateObject<TObject>(string assemblyName, string typeName, params object[] args)
        {
            try
            {
#if !NO_APPDOMAIN
                object unwrappedObject = AppDomain.CreateInstanceAndUnwrap(assemblyName, typeName, false, 0, null, args, null, null, null);
                return (TObject)unwrappedObject;
#else
                var objHandle = Activator.CreateInstance(AppDomain.CurrentDomain, assemblyName, typeName, false, BindingFlags.Default, null, args, null, null);
                return (TObject)objHandle.Unwrap();                    
#endif
                
            }
            catch (TargetInvocationException ex)
            {
                ex.InnerException.RethrowWithNoStackTraceLoss();
                return default(TObject);
            }
        }

        public virtual void Dispose()
        {
#if !NO_APPDOMAIN
            if (AppDomain != null)
            {
                string cachePath = AppDomain.SetupInformation.CachePath;

                try
                {
                    System.AppDomain.Unload(AppDomain);

                    if (cachePath != null)
                        Directory.Delete(cachePath, true);
                }
                catch { }
            }
#endif
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