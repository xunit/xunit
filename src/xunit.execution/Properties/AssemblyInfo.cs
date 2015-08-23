using System;
using System.Reflection;
using Xunit.Sdk;

#if PLATFORM_DOTNET
[assembly: AssemblyTitle("xUnit.net Execution (dotnet)")]
#else
[assembly: AssemblyTitle("xUnit.net Execution (desktop)")]
#endif

[assembly: CLSCompliant(true)]
[assembly: PlatformSpecificAssembly]
