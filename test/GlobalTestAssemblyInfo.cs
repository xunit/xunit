using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

#if !NET40
[assembly: AssemblyTrait("Assembly", "Trait")]
#endif

#if NET452
[assembly: TestDriven.Framework.CustomTestRunner(typeof(Xunit.Runner.TdNet.TdNetRunner))]
#endif
