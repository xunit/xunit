using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class CollectionBehaviorAttributeGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.CollectionBehaviorAttribute)
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).SetTestCollectionFactory(attribute);
}

[Generator(LanguageNames.CSharp)]
public class CollectionBehaviorAttributeOfTGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.CollectionBehaviorAttribute + "`1")
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute) =>
			(registration ?? throw new ArgumentNullException(nameof(registration))).SetTestCollectionFactory(attribute);
}
