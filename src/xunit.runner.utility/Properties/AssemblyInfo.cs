using System;
using System.Reflection;

#if NET35
[assembly: AssemblyTitle("xUnit.net Runner Utility (.NET 3.5)")]
#elif NET452
[assembly: AssemblyTitle("xUnit.net Runner Utility (.NET 4.5.2)")]
#elif NETSTANDARD1_1
[assembly: AssemblyTitle("xUnit.net Runner Utility (.NET Standard 1.1)")]
#elif NETSTANDARD1_5
[assembly: AssemblyTitle("xUnit.net Runner Utility (.NET Standard 1.5)")]
#elif NETSTANDARD2_0
[assembly: AssemblyTitle("xUnit.net Runner Utility (.NET Standard 2.0)")]
#elif NETCOREAPP1_0
[assembly: AssemblyTitle("xUnit.net Runner Utility (.NET Core 1.0)")]
#elif WINDOWS_UAP
[assembly: AssemblyTitle("xUnit.net Runner Utility (Universal Windows 10.0)")]
#else
#error Unknown target platform
#endif

[assembly: CLSCompliant(true)]
