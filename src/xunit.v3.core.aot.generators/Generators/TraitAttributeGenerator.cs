using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TraitAttributeGenerator() :
	XunitAssemblyAttributeGenerator(Types.Xunit.TraitAttribute)
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		CodeGenTestAssemblyRegistration registration,
		AttributeData attribute)
	{
		if (semanticModel is null)
			throw new ArgumentNullException(nameof(semanticModel));
		if (registration is null)
			throw new ArgumentNullException(nameof(registration));
		if (attribute is null)
			throw new ArgumentNullException(nameof(attribute));

		if (attribute.ConstructorArguments.Length != 2)
			return;

		if (attribute.ConstructorArguments[0].Kind != TypedConstantKind.Primitive
				|| attribute.ConstructorArguments[1].Kind != TypedConstantKind.Primitive
				|| attribute.ConstructorArguments[0].Value is not string name
				|| attribute.ConstructorArguments[1].Value is not string value)
			return;

		registration.AddTrait(name, value);
	}
}
