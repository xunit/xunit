using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class InlineDataAttributeGenerator() :
	DataAttributeGenerator(Types.Xunit.InlineDataAttribute)
{
	protected override void ProcessAttribute(
		SemanticModel semanticModel,
		INamedTypeSymbol testClass,
		IMethodSymbol testMethod,
		AttributeData attribute,
		DataAttributeGeneratorResult result,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(semanticModel);
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(attribute);
		Guard.ArgumentNotNull(result);

		if (attribute.ConstructorArguments.Length < 1)
			return;

		var dataAttributeRegistration = DataAttributeRegistration.TryGenerate<DataAttributeRegistration>(semanticModel, testClass, testMethod, attribute);
		if (dataAttributeRegistration is null)
			return;

		result.Factories.Add(new($$"""
			async disposalTracker => {
				var attr = {{dataAttributeRegistration}};
				var data = {{(attribute.ConstructorArguments[0].IsNull ? "new object?[] { null }" : attribute.ConstructorArguments[0].ToCSharp())}};
				return new[] { attr.CreateDataRow(data) };
			}
			""", false));
	}
}
