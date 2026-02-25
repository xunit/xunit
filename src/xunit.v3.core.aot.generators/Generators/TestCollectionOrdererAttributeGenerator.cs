using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TestCollectionOrdererAttributeGenerator() :
	AssemblyFactoryAttributeGeneratorBase(
		Types.Xunit.TestCollectionOrdererAttribute,
		nameof(Types.Xunit.TestCollectionOrdererAttribute),
		"RegisterAssemblyTestCollectionOrdererFactory")
{
	protected override bool ValidateImplementationType(
		INamedTypeSymbol type,
		Location? location,
		GeneratorResult result) =>
			EnsureImplementsInterface(type, location, result, Types.Xunit.v3.ITestCollectionOrderer);
}
