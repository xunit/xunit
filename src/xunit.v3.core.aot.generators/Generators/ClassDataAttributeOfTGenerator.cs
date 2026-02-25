using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class ClassDataAttributeOfTGenerator() :
	ClassDataAttributeGeneratorBase(Types.Xunit.ClassDataAttribute + "`1")
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

		if (attribute.AttributeClass?.TypeArguments.Length < 1 || attribute.AttributeClass?.TypeArguments[0] is not INamedTypeSymbol classDataType)
			return;

		ProcessClassDataAttribute(classSymbol, methodSymbol, attribute, classDataType, dataAttributeRegistration, result);
	}
}
