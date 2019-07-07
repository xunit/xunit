using System.Reflection;

#if NET472
[assembly: AssemblyTitle("xUnit.net Console Test Runner (.NET 4.7.2)")]
#elif NETCOREAPP2_1
[assembly: AssemblyTitle("xUnit.net Console Test Runner (.NET Core 2.1)")]
#else
#error Unknown target platform
#endif
