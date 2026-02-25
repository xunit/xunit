using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class AssemblyFixtureAttributeGenerator() :
	AssemblyFactoryAttributeGeneratorBase(
		Types.Xunit.AssemblyFixtureAttribute,
		nameof(Types.Xunit.AssemblyFixtureAttribute))
{
	protected override string GetRegistration(GeneratorResult result) =>
		$"global::Xunit.v3.RegisteredEngineConfig.RegisterAssemblyFixtureFactory(typeof({Guard.ArgumentNotNull(result).Type}), async () => {result.Factory});";
}
