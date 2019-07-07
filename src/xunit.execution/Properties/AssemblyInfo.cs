using System;
using System.Reflection;
using Xunit.Sdk;

#if NETSTANDARD
[assembly: AssemblyTitle("xUnit.net Execution (dotnet)")]
#else
#error Unknown target platform
#endif

[assembly: CLSCompliant(true)]
[assembly: PlatformSpecificAssembly]
