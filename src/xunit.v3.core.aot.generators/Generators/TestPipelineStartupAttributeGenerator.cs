using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TestPipelineStartupAttributeGenerator() :
	AssemblyFactoryAttributeGeneratorBase(
		Types.Xunit.v3.TestPipelineStartupAttribute,
		nameof(Types.Xunit.v3.TestPipelineStartupAttribute),
		"RegisterTestPipelineStartupFactory")
{
	protected override bool ValidateImplementationType(
		INamedTypeSymbol type,
		Location? location,
		GeneratorResult result) =>
			EnsureImplementsInterface(type, location, result, Types.Xunit.v3.ITestPipelineStartup);
}
