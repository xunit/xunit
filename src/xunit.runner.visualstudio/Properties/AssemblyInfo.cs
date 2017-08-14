using System.Reflection;

#if NET452
[assembly: AssemblyTitle("xUnit.net Runner for Visual Studio (Desktop .NET)")]
#elif NETCOREAPP1_0
[assembly: AssemblyTitle("xUnit.net Runner for Visual Studio (.NET Core)")]
#elif WINDOWS_UAP
[assembly: AssemblyTitle("xUnit.net Runner for Visual Studio (Universal Windows)")]
#else
#error Unknown target platform
#endif
