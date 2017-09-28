using System.Reflection;

#if NET452
[assembly: AssemblyTitle("xUnit.net Runner for Visual Studio (.NET 4.5.2)")]
#elif NETCOREAPP1_0
[assembly: AssemblyTitle("xUnit.net Runner for Visual Studio (.NET Core 1.0)")]
#elif WINDOWS_UAP
[assembly: AssemblyTitle("xUnit.net Runner for Visual Studio (Universal Windows 10.0)")]
#else
#error Unknown target platform
#endif
