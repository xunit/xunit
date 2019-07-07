using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

[assembly: AssemblyVersion("99.99.99.0")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

#if !NET45
[assembly: AssemblyTrait("Assembly", "Trait")]
#endif

#if NET472
[assembly: TestDriven.Framework.CustomTestRunner(typeof(Xunit.Runner.TdNet.TdNetRunner))]
#endif
