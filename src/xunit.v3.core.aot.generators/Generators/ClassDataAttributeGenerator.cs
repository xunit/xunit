using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class ClassDataAttributeGenerator() :
	ClassDataAttributeGeneratorBase(Types.Xunit.ClassDataAttribute)
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		INamedTypeSymbol testClass,
		IMethodSymbol testMethod,
		AttributeData attribute,
		DataAttributeGeneratorResult result,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(attribute);

		if (attribute.ConstructorArguments.Length < 1 || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol classDataType)
			return;

		var dataAttributeRegistration = DataAttributeRegistration.TryGenerate<DataAttributeRegistration>(semanticModel, testClass, testMethod, attribute);
		if (dataAttributeRegistration is null)
			return;

		ProcessClassDataAttribute(semanticModel, testClass, testMethod, attribute, classDataType, dataAttributeRegistration, result);
	}
}

[Generator(LanguageNames.CSharp)]
public class ClassDataAttributeOfTGenerator() :
	ClassDataAttributeGeneratorBase(Types.Xunit.ClassDataAttribute + "`1")
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		INamedTypeSymbol testClass,
		IMethodSymbol testMethod,
		AttributeData attribute,
		DataAttributeGeneratorResult result,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(attribute);

		if (attribute.AttributeClass?.TypeArguments.Length < 1 || attribute.AttributeClass?.TypeArguments[0] is not INamedTypeSymbol classDataType)
			return;

		var dataAttributeRegistration = DataAttributeRegistration.TryGenerate<DataAttributeRegistration>(semanticModel, testClass, testMethod, attribute);
		if (dataAttributeRegistration is null)
			return;

		ProcessClassDataAttribute(semanticModel, testClass, testMethod, attribute, classDataType, dataAttributeRegistration, result);
	}
}
