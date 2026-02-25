using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class InlineDataAttributeGenerator() :
	DataAttributeGeneratorBase(Types.Xunit.InlineDataAttribute)
{
	protected override void ProcessAttribute(
		INamedTypeSymbol classSymbol,
		IMethodSymbol methodSymbol,
		AttributeData attribute,
		string dataAttributeRegistration,
		GeneratorResult result,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(classSymbol);
		Guard.ArgumentNotNull(methodSymbol);
		Guard.ArgumentNotNull(attribute);
		Guard.ArgumentNotNull(dataAttributeRegistration);
		Guard.ArgumentNotNull(result);

		if (attribute.ConstructorArguments.Length < 1)
			return;

		result.GeneratorSuffix = $"{classSymbol.Name}٠{methodSymbol.Name}٠";

		result.Factories.Add($$"""
			async disposalTracker => {
				var attr = {{dataAttributeRegistration}};
				var data = {{(attribute.ConstructorArguments[0].IsNull ? "new object?[] { null }" : attribute.ConstructorArguments[0].ToCSharp())}};
				return new[] { attr.CreateDataRow(data) };
			}
			""");
	}
}
