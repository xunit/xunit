using System;
using System.Reflection;
using Xunit.Sdk;

#if NET452
[assembly: AssemblyTitle("xUnit.net Execution (desktop)")]
#elif NETSTANDARD
[assembly: AssemblyTitle("xUnit.net Execution (dotnet)")]
#else
#error Unknown target platform
#endif

[assembly: CLSCompliant(true)]
[assembly: PlatformSpecificAssembly]
