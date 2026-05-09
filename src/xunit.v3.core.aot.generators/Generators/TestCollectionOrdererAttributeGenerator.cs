using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TestCollectionOrdererAttributeGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.TestCollectionOrdererAttribute)
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).SetTestCollectionOrderer(attribute);
}

[Generator(LanguageNames.CSharp)]
public class TestCollectionOrdererAttributeOfTGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.TestCollectionOrdererAttribute + "`1")
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).SetTestCollectionOrderer(attribute);
}
