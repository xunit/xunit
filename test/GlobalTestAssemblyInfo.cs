using System.Reflection;
using Xunit;

[assembly: AssemblyVersion("99.99.99.0")]
[assembly: AssemblyTrait("Assembly", "Trait")]

#if NET452
[assembly: TestDriven.Framework.CustomTestRunner(typeof(Xunit.Runner.TdNet.TdNetRunner))]
#endif
