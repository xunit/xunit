using System;
using System.Reflection;

namespace Xunit
{
    class AppDomainManager_NoAppDomain : IAppDomainManager
    {
        readonly string assemblyFileName;

        public AppDomainManager_NoAppDomain(string assemblyFileName)
        {
            this.assemblyFileName = assemblyFileName;
        }

        public bool HasAppDomain => false;

        public TObject CreateObject<TObject>(AssemblyName assemblyName, string typeName, params object[] args)
        {
            try
            {
#if NETFRAMEWORK
                var type = Assembly.Load(assemblyName).GetType(typeName, throwOnError: true);
#else
                var type = Type.GetType($"{typeName}, {assemblyName.FullName}", throwOnError: true);
#endif
                return (TObject)Activator.CreateInstance(type, args);
            }
            catch (TargetInvocationException ex)
            {
                ex.InnerException.RethrowWithNoStackTraceLoss();
                return default(TObject);
            }
        }

#if NETFRAMEWORK
        public TObject CreateObjectFrom<TObject>(string assemblyLocation, string typeName, params object[] args)
        {
            try
            {
                var type = Assembly.LoadFrom(assemblyLocation).GetType(typeName, throwOnError: true);
                return (TObject)Activator.CreateInstance(type, args);
            }
            catch (TargetInvocationException ex)
            {
                ex.InnerException.RethrowWithNoStackTraceLoss();
                return default(TObject);
            }
        }
#endif
        public void Dispose() { }
    }
}
