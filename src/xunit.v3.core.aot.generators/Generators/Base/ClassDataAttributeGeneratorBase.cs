using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

public abstract class ClassDataAttributeGeneratorBase(string fullyQualifiedAttributeType) :
	DataAttributeGenerator(fullyQualifiedAttributeType)
{
	protected static void ProcessClassDataAttribute(
		SemanticModel semanticModel,
		INamedTypeSymbol testClass,
		IMethodSymbol testMethod,
		AttributeData attribute,
		INamedTypeSymbol classDataType,
		string dataAttributeRegistration,
		DataAttributeGeneratorResult result)
	{
		Guard.ArgumentNotNull(semanticModel);
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(attribute);
		Guard.ArgumentNotNull(classDataType);
		Guard.ArgumentNotNull(dataAttributeRegistration);
		Guard.ArgumentNotNull(result);

		var objectType = semanticModel.Compilation.GetSpecialType(SpecialType.System_Object);

		if (classDataType.DeclaredAccessibility != Accessibility.Public || classDataType.IsAbstract)
			return;

		if (!classDataType.Constructors.Any(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic && c.Parameters.Length == 0))
			return;

		var theoryDataInfo = classDataType.GetTheoryDataInfo(objectType);
		if (theoryDataInfo is null)
			return;

		var foreachAwait = theoryDataInfo.IsAsyncEnumerable ? "await " : "";
		var dataRowAwait = theoryDataInfo.IsAsync ? "await " : "";
		var asyncClassDataInit =
			classDataType.ImplementsInterface(Types.Xunit.IAsyncLifetime)
				? "await ((global::Xunit.IAsyncLifetime)classData).InitializeAsync();\n\t"
				: string.Empty;

		result.Factories.Add(new($$"""
			async disposalTracker => {
				var attr = {{dataAttributeRegistration}};
				var dataRows = new global::System.Collections.Generic.List<global::Xunit.ITheoryDataRow>();
				var classData = new {{classDataType.ToCSharp()}}();
				disposalTracker.Add(classData);
				{{asyncClassDataInit}}{{foreachAwait}}foreach (var dataRow in {{dataRowAwait}}classData)
					dataRows.Add(attr.CreateDataRow(dataRow));
				return dataRows;
			}
			""", false));
	}
}
