using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TestCaseOrdererAttributeGenerator() :
	AssemblyFactoryAttributeGeneratorBase(
		Types.Xunit.TestCaseOrdererAttribute,
		nameof(Types.Xunit.TestCaseOrdererAttribute),
		"RegisterAssemblyTestCaseOrdererFactory")
{
	protected override bool ValidateImplementationType(
		INamedTypeSymbol type,
		Location? location,
		GeneratorResult result) =>
			EnsureImplementsInterface(type, location, result, Types.Xunit.v3.ITestCaseOrderer);
}
