using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

// This file does not use GlobalAssemblyInfo.cs because its version should not change over time.
// Once 2.0 ships, this assembly will be considered to be frozen.

#if NETSTANDARD1_0
[assembly: AssemblyTitle("xUnit.net Abstractions (.NET Standard 1.0)")]
#elif NET35
[assembly: AssemblyTitle("xUnit.net Abstractions (.NET 3.5)")]
#else
[assembly: AssemblyTitle("xUnit.net Abstractions (PCL259)")]
#endif

[assembly: AssemblyCompany(".NET Foundation")]
[assembly: AssemblyProduct("xUnit.net Testing Framework")]
[assembly: AssemblyCopyright("Copyright (C) .NET Foundation")]
[assembly: CLSCompliant(true)]
[assembly: AssemblyVersion("2.0.1.0")]

[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "xunit")]
