using System.Diagnostics.CodeAnalysis;
using System.Reflection;

[assembly: AssemblyCompany("Outercurve Foundation")]
[assembly: AssemblyProduct("xUnit.net Testing Framework")]
[assembly: AssemblyCopyright("Copyright (C) Outercurve Foundation")]
[assembly: AssemblyVersion("99.99.99.0")]

[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Xunit.Sdk")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "xunit")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "extensions")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "utility")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "runner")]