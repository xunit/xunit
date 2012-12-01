using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using Xunit.Abstractions;

namespace Xunit
{
    public abstract class AppDomainXunitController : LongLivedMarshalByRefObject, IXunitController
    {
        protected AppDomainXunitController(string assemblyFileName, string configFileName, bool shadowCopy, string testFrameworkFileName)
        {
            Guard.ArgumentValid("testFrameworkFileName", "Could not find file: " + testFrameworkFileName, File.Exists(testFrameworkFileName));

            AssemblyFileName = assemblyFileName;
            ConfigFileName = configFileName;
            AppDomain = CreateAppDomain(assemblyFileName, configFileName, shadowCopy);
            XunitAssemblyName = AssemblyName.GetAssemblyName(testFrameworkFileName);
        }

        protected AppDomain AppDomain { get; private set; }

        public string AssemblyFileName { get; private set; }

        public string ConfigFileName { get; private set; }

        protected AssemblyName XunitAssemblyName { get; private set; }

        public Version XunitVersion
        {
            get { return XunitAssemblyName.Version; }
        }

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

        protected TObject CreateObject<TObject>(string typeName, params object[] args)
        {
            try
            {
                return (TObject)AppDomain.CreateInstanceAndUnwrap(XunitAssemblyName.FullName, typeName, false, 0, null, args, null, null);
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

                AppDomain.Unload(AppDomain);

                try
                {
                    if (cachePath != null)
                        Directory.Delete(cachePath, true);
                }
                catch { }
            }
        }

        public abstract void Find(bool includeSourceInformation, IMessageSink messageSink);

        public abstract void Find(ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink);

        public abstract void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink);
    }
}
