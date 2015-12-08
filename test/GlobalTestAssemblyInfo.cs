using System.Reflection;
using TestDriven.Framework;
using Xunit;
using Xunit.Runner.TdNet;

[assembly: CustomTestRunner(typeof(TdNetRunner))]
[assembly: AssemblyVersion("99.99.99.0")]

[assembly: AssemblyTrait("Assembly", "Trait")]