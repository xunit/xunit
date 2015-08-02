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
#if NETFX_CORE || WINDOWS_PHONE || DOTNETCORE
                var type = Type.GetType($"{typeName}, {assemblyName.FullName}", true);
                return (TObject)Activator.CreateInstance(type, args);
#else
                return (TObject)Activator.CreateInstance(Assembly.Load(assemblyName).GetType(typeName), args);
#endif
            }
            catch (TargetInvocationException ex)
            {
                ex.InnerException.RethrowWithNoStackTraceLoss();
                return default(TObject);
            }
        }

        public void Dispose() { }
    }
}
