using System.Reflection;

#if NET452
[assembly: AssemblyTitle("xUnit.net Console Test Runner (.NET 4.5.2)")]
#elif NET46
[assembly: AssemblyTitle("xUnit.net Console Test Runner (.NET 4.6)")]
#elif NET461
[assembly: AssemblyTitle("xUnit.net Console Test Runner (.NET 4.6.1)")]
#elif NET462
[assembly: AssemblyTitle("xUnit.net Console Test Runner (.NET 4.6.2)")]
#elif NET47
[assembly: AssemblyTitle("xUnit.net Console Test Runner (.NET 4.7)")]
#elif NET471
[assembly: AssemblyTitle("xUnit.net Console Test Runner (.NET 4.7.1)")]
#elif NET472
[assembly: AssemblyTitle("xUnit.net Console Test Runner (.NET 4.7.2)")]
#elif NETCOREAPP1_0
[assembly: AssemblyTitle("xUnit.net Console Test Runner (.NET Core 1.x)")]
#elif NETCOREAPP2_0
[assembly: AssemblyTitle("xUnit.net Console Test Runner (.NET Core 2.x)")]
#else
#error Unknown target platform
#endif
