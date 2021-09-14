using System.Reflection;

[assembly: AssemblyCompany(".NET Foundation")]
[assembly: AssemblyProduct("xUnit.net Testing Framework")]
[assembly: AssemblyCopyright("Copyright (C) .NET Foundation")]

#if NET35 || NETCOREAPP1_0
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class ExcludeFromCodeCoverageAttribute : Attribute { }
}
#endif
