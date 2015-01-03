﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
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
            Guard.FileExists("assemblyFileName", assemblyFileName);

            if (configFileName == null)
                configFileName = GetDefaultConfigFile(assemblyFileName);

            if (configFileName != null)
                configFileName = Path.GetFullPath(configFileName);

            AssemblyFileName = assemblyFileName;
            ConfigFileName = configFileName;
            AppDomain = CreateAppDomain(assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);
        }

        public AppDomain AppDomain { get; private set; }

        public string AssemblyFileName { get; private set; }

        public string ConfigFileName { get; private set; }

        static AppDomain CreateAppDomain(string assemblyFilename, string configFilename, bool shadowCopy, string shadowCopyFolder)
        {
            var setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(assemblyFilename);
            setup.ApplicationName = Guid.NewGuid().ToString() + Path.GetFileNameWithoutExtension(assemblyFilename);

            if (shadowCopy)
            {
                setup.ShadowCopyFiles = "true";
                setup.ShadowCopyDirectories = setup.ApplicationBase;
                setup.CachePath = shadowCopyFolder ?? Path.Combine(Path.GetTempPath(), setup.ApplicationName);
            }

            setup.ConfigurationFile = configFilename;

            return AppDomain.CreateDomain(Path.GetFileNameWithoutExtension(assemblyFilename), System.AppDomain.CurrentDomain.Evidence, setup, new PermissionSet(PermissionState.Unrestricted));
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

        public TObject CreateObjectFromPath<TObject>(string assemblyPath, string typeName, params object[] args)
        {
            try
            {
                object unwrappedObject = AppDomain.CreateInstanceFromAndUnwrap(assemblyPath, typeName, false, 0, null, args, null, null, null);
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

                try
                {
                    System.AppDomain.Unload(AppDomain);

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