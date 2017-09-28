using System.Reflection;

#if NET452
[assembly: AssemblyTitle("xUnit.net Console Test Runner (.NET 4.5.2)")]
#elif NETCOREAPP1_0
[assembly: AssemblyTitle("xUnit.net Console Test Runner (.NET Core 1.x)")]
#elif NETCOREAPP2_0
[assembly: AssemblyTitle("xUnit.net Console Test Runner (.NET Core 2.x)")]
#else
#error Unknown target platform
#endif
