using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TestClassOrdererAttributeGenerator() :
	AssemblyFactoryAttributeGeneratorBase(
		Types.Xunit.TestClassOrdererAttribute,
		nameof(Types.Xunit.TestClassOrdererAttribute),
		"RegisterAssemblyTestClassOrdererFactory")
{
	protected override bool ValidateImplementationType(
		INamedTypeSymbol type,
		Location? location,
		GeneratorResult result) =>
			EnsureImplementsInterface(type, location, result, Types.Xunit.v3.ITestClassOrderer);
}
