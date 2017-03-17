using System;
using System.Reflection;

namespace Xunit
{
    interface IAppDomainManager : IDisposable
    {
        TObject CreateObject<TObject>(AssemblyName assemblyName, string typeName, params object[] args);
#if NET35 || NET452
        TObject CreateObjectFrom<TObject>(string assemblyLocation, string typeName, params object[] args);
#endif
    }
}
