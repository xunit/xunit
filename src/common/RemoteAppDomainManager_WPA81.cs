using System;
using System.Reflection;

namespace Xunit
{
    internal class RemoteAppDomainManager : IDisposable
    {
        public RemoteAppDomainManager(string assemblyFileName, string configFileName, bool shadowCopy, string shadowCopyFolder)
        {
            Guard.ArgumentNotNullOrEmpty("assemblyFileName", assemblyFileName);
            Guard.FileExists("assemblyFileName", assemblyFileName);

            AssemblyFileName = assemblyFileName;
            ConfigFileName = configFileName;
        }

        public string AssemblyFileName { get; private set; }

        public string ConfigFileName { get; private set; }

        public TObject CreateObject<TObject>(string assemblyName, string typeName, params object[] args)
        {
            try
            {
                var type = Type.GetType(typeName + ", " + assemblyName, true);
                return (TObject)Activator.CreateInstance(type, args);
            }
            catch (TargetInvocationException ex)
            {
                ex.InnerException.RethrowWithNoStackTraceLoss();
                return default(TObject);
            }
        }

        public virtual void Dispose() { }
    }
}