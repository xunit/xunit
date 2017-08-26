using System.Reflection;
using Xunit;

[assembly: AssemblyVersion("99.99.99.0")]

#if !NET40
[assembly: AssemblyTrait("Assembly", "Trait")]
#endif

#if NET452
[assembly: TestDriven.Framework.CustomTestRunner(typeof(Xunit.Runner.TdNet.TdNetRunner))]
#endif
