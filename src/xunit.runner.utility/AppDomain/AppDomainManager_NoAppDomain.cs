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

        public TObject CreateObject<TObject>(AssemblyName assemblyName, string typeName, params object[] args)
        {
            try
            {
#if NET35 || NET452
                var type = Assembly.Load(assemblyName).GetType(typeName);
#else
                var type = Type.GetType($"{typeName}, {assemblyName.FullName}", true);
#endif
                return (TObject)Activator.CreateInstance(type, args);
            }
            catch (TargetInvocationException ex)
            {
                ex.InnerException.RethrowWithNoStackTraceLoss();
                return default(TObject);
            }
        }

#if NET35 || NET452
        public TObject CreateObjectFrom<TObject>(string assemblyLocation, string typeName, params object[] args)
        {
            try
            {
                var type = Assembly.LoadFrom(assemblyLocation).GetType(typeName);
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
