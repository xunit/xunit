using System.Reflection;
using TestDriven.Framework;
using Xunit;
using Xunit.Runner.TdNet;

[assembly: CustomTestRunner(typeof(TdNetRunner))]
[assembly: AssemblyVersion("99.99.99.0")]

[assembly: AssemblyTrait("Assembly", "Trait")]
[assembly: NamespaceTrait("OuterNamespace", "OuterNamespace", "Trait")]
[assembly: NamespaceTrait("outerNamespace", "outerNamespace", "Trait")]
[assembly: NamespaceTrait("outerNamespace", "ciOuterNamespace", "Trait", NamespaceCaseInsensitive = true, NotApplicableToNestedNamespaces = true)]

[assembly: NamespaceTrait("OuterNamespace.InnerNamespace", "InnerNamespace", "Trait")]

