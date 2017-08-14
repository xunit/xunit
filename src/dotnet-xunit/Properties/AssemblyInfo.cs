using System.Reflection;

#if NETCOREAPP1_0
[assembly: AssemblyTitle("xUnit.net Runner for .NET Core (1.x)")]
#elif NETCOREAPP2_0
[assembly: AssemblyTitle("xUnit.net Runner for .NET Core (2.x)")]
#else
#error Unknown target platform
#endif
