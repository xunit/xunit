using System;
using System.Reflection;

namespace Xunit
{
    interface IAppDomainManager : IDisposable
    {
        TObject CreateObject<TObject>(AssemblyName assemblyName, string typeName, params object[] args);
    }
}
