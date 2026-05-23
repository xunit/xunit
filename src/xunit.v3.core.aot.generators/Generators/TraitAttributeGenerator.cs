using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class TraitAttributeGenerator() :
	TraitGenerator(Types.Xunit.TraitAttribute)
{
	protected override IEnumerable<(string name, string value)> GetTraitValues(AttributeData attribute)
	{
		Guard.ArgumentNotNull(attribute);

		if (attribute.ConstructorArguments.Length == 2
				&& attribute.ConstructorArguments[0].Kind == TypedConstantKind.Primitive
				&& attribute.ConstructorArguments[1].Kind == TypedConstantKind.Primitive
				&& attribute.ConstructorArguments[0].Value is string name
				&& attribute.ConstructorArguments[1].Value is string value)
			yield return (name, value);
	}
}
