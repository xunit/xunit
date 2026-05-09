using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TestFrameworkAttributeGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.TestFrameworkAttribute)
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).SetTestFramework(attribute);
}

[Generator(LanguageNames.CSharp)]
public class TestFrameworkAttributeOfTGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.TestFrameworkAttribute + "`1")
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).SetTestFramework(attribute);
}
