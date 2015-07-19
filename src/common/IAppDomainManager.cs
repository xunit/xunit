using System;
using System.Reflection;

namespace Xunit
{
    internal interface IAppDomainManager : IDisposable
    {
        TObject CreateObject<TObject>(AssemblyName assemblyName, string typeName, params object[] args);
    }
}
