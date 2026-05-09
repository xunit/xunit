using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TestClassOrdererAttributeGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.TestClassOrdererAttribute)
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).SetTestClassOrderer(attribute);
}

[Generator(LanguageNames.CSharp)]
public class TestClassOrdererAttributeOfTGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.TestClassOrdererAttribute + "`1")
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).SetTestClassOrderer(attribute);
}
