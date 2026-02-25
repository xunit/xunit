using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class ClassDataAttributeGenerator() :
	ClassDataAttributeGeneratorBase(Types.Xunit.ClassDataAttribute)
{
	protected override void ProcessAttribute(
		INamedTypeSymbol classSymbol,
		IMethodSymbol methodSymbol,
		AttributeData attribute,
		string dataAttributeRegistration,
		GeneratorResult result,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(attribute);

		if (attribute.ConstructorArguments.Length < 1 || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol classDataType)
			return;

		ProcessClassDataAttribute(classSymbol, methodSymbol, attribute, classDataType, dataAttributeRegistration, result);
	}
}
