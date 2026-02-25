using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TestMethodOrdererAttributeGenerator() :
	AssemblyFactoryAttributeGeneratorBase(
		Types.Xunit.TestMethodOrdererAttribute,
		nameof(Types.Xunit.TestMethodOrdererAttribute),
		"RegisterAssemblyTestMethodOrdererFactory")
{
	protected override bool ValidateImplementationType(
		INamedTypeSymbol type,
		Location? location,
		GeneratorResult result) =>
			EnsureImplementsInterface(type, location, result, Types.Xunit.v3.ITestMethodOrderer);
}
