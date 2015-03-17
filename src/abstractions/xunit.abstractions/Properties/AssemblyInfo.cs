using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

// This file does not use GlobalAssemblyInfo.cs because its version should not change over time.
// Once 2.0 ships, this assembly will be considered to be frozen.

#if PCL
[assembly: AssemblyTitle("xUnit.net Abstractions (PCL)")]
#else
[assembly: AssemblyTitle("xUnit.net Abstractions (.NET 3.5)")]
#endif

[assembly: AssemblyCompany("Outercurve Foundation")]
[assembly: AssemblyProduct("xUnit.net Testing Framework")]
[assembly: AssemblyCopyright("Copyright (C) Outercurve Foundation")]
[assembly: CLSCompliant(true)]
[assembly: AssemblyVersion("2.0.0.0")]

[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "xunit")]
