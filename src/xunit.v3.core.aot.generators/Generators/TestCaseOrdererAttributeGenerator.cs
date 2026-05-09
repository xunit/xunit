using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TestCaseOrdererAttributeGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.TestCaseOrdererAttribute)
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).SetTestCaseOrderer(attribute);
}

[Generator(LanguageNames.CSharp)]
public class TestCaseOrdererAttributeOfTGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.TestCaseOrdererAttribute + "`1")
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).SetTestCaseOrderer(attribute);
}
