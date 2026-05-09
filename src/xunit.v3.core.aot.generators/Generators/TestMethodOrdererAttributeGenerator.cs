using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TestMethodOrdererAttributeGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.TestMethodOrdererAttribute)
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).SetTestMethodOrderer(attribute);
}

[Generator(LanguageNames.CSharp)]
public class TestMethodOrdererAttributeOfTGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.TestMethodOrdererAttribute + "`1")
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).SetTestMethodOrderer(attribute);
}
