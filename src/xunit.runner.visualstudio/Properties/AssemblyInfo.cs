using System.Reflection;

[assembly: AssemblyCompany(".NET Foundation")]
[assembly: AssemblyProduct("xUnit.net Testing Framework")]
[assembly: AssemblyCopyright("Copyright (C) .NET Foundation")]
[assembly: AssemblyVersion("99.99.99.0")]
[assembly: AssemblyFileVersion("99.99.99.0")]
[assembly: AssemblyInformationalVersionAttribute("99.99.99-dev")]

#if NET452
[assembly: AssemblyTitle("xUnit.net Runner for Visual Studio (Desktop .NET)")]
#elif NETCOREAPP1_0
[assembly: AssemblyTitle("xUnit.net Runner for Visual Studio (.NET Core)")]
#elif WINDOWS_UAP
[assembly: AssemblyTitle("xUnit.net Runner for Visual Studio (Universal Windows)")]
#else
#error Unknown target platform
#endif
