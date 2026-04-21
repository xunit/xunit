using System.Text;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class MemberDataAttributeGenerator() :
	DataAttributeGeneratorBase(Types.Xunit.MemberDataAttribute)
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

		if (attribute.ConstructorArguments.Length < 1 || attribute.ConstructorArguments[0].Value is not string memberName)
			return;

		var disableDiscoveryEnumeration = false;
		ITypeSymbol memberType = classSymbol;

		foreach (var namedArgument in attribute.NamedArguments)
			switch (namedArgument.Key)
			{
				case Names.Xunit.v3.MemberDataAttributeBase.DisableDiscoveryEnumeration:
					disableDiscoveryEnumeration = namedArgument.Value.Value is true;
					break;

				case Names.Xunit.v3.MemberDataAttributeBase.MemberType:
					memberType = namedArgument.Value.Value as ITypeSymbol ?? memberType;
					break;
			}

		var location = attribute.ApplicationSyntaxReference.Location;
		var member = default(ISymbol);

		for (var currentType = memberType; currentType is not null; currentType = currentType.BaseType)
		{
			var members = currentType.GetMembers().Where(m => m.Name == memberName).ToArray();
			if (members.Length == 0)
				continue;
			if (members.Length > 1 || currentType.DeclaredAccessibility is not Accessibility.Public and not Accessibility.Internal)
				return;

			member = members[0];
			break;
		}

		if (member is null || member.DeclaredAccessibility != Accessibility.Public || !member.IsStatic)
			return;

		var returnType = member switch
		{
			IMethodSymbol method => method.ReturnType,
			IPropertySymbol property => property.Type,
			IFieldSymbol field => field.Type,
			_ => null,
		};

		if (returnType is null)
			return;

		if (memberType is INamedTypeSymbol namedMemberType)
			if (namedMemberType.IsGenericType && namedMemberType.TypeParameters.Any(t => t.Kind == SymbolKind.TypeParameter))
				return;

		var theoryDataInfo = returnType.GetTheoryDataInfo(result.ObjectType);
		if (theoryDataInfo is null)
			return;

		if (member is IPropertySymbol memberProperty)
			if (memberProperty.GetMethod is null || memberProperty.GetMethod.DeclaredAccessibility != Accessibility.Public)
				return;

		var parameters = string.Empty;
		var parametersInit = new StringBuilder();
		var arguments = attribute.ConstructorArguments[1].Values;
		if (member is not IMethodSymbol memberMethod)
		{
			if (arguments.Length != 0)
				return;
		}
		else
		{
			if (arguments.Length > memberMethod.Parameters.Length || memberMethod.Parameters.Any(p => p.IsParams))
				return;

			var requiredParameters = memberMethod.Parameters.Where(p => !p.IsOptional).ToArray();
			if (arguments.Length < requiredParameters.Length)
				return;

			var parameterNamesInCode = new List<string>();

			if (arguments.Length > 0)
			{
				parametersInit.Append("""
				var invalidParameters = new global::System.Collections.Generic.List<(string Type, string Name, string Value)>();

				""");

				for (var idx = 0; idx < memberMethod.Parameters.Length; ++idx)
				{
					var parameter = memberMethod.Parameters[idx];
					var parameterName = parameter.Name.Quoted();
					var parameterNameInCode = "param" + idx;

					if (idx >= arguments.Length)
					{
						if (!parameter.IsOptional && !parameter.IsParams)
							parametersInit.Append($$"""
									invalidParameters.Add(({{parameter.Type.ToDisplayString().Quoted()}}, {{parameterName}}, "<missing value>"));

								""");
					}
					else
					{
						var argument = arguments[idx];
						var conversion = parameter.NullableAnnotation == NullableAnnotation.NotAnnotated ? "TryConvert" : "TryConvertNullable";

						parameterNamesInCode.Add(parameterNameInCode);
						parametersInit.Append($$"""
								if (!global::Xunit.Sdk.TypeHelper.{{conversion}}({{argument.ToCSharp()}}, out {{parameter.Type.ToCSharp()}} {{parameterNameInCode}}))
									invalidParameters.Add(({{parameter.Type.ToDisplayString().Quoted()}}, {{parameterName}}, {{argument.ToCSharp().Quoted()}}));

							""");
					}
				}

				parametersInit.Append($$"""
						if (invalidParameters.Count != 0)
							throw new global::Xunit.Sdk.TestPipelineException(
								string.Format(
									global::System.Globalization.CultureInfo.CurrentCulture,
									"Member data method '{{memberType.ToDisplayString()}}.{{memberMethod.Name}}' had one or more invalid theory data arguments: {0}",
									string.Join(", ", global::System.Linq.Enumerable.Select(invalidParameters, a => $"{a.Type} {a.Name} ({a.Value})"))
								)
							);

					""" + "\t");
			}

			parameters = $"({string.Join(", ", parameterNamesInCode)})";
		}

		result.GeneratorSuffix = $"{classSymbol.Name}٠{methodSymbol.Name}٠";

		var factory = new StringBuilder();

		var foreachAwait = theoryDataInfo.Value.IsAsyncEnumerable ? "await " : "";
		var dataRowAwait = theoryDataInfo.Value.IsTask ? "await " : "";

		factory.Append($$"""
			async disposalTracker => {
				{{parametersInit}}var attr = {{dataAttributeRegistration}};
				var result = new global::System.Collections.Generic.List<global::Xunit.ITheoryDataRow>();
				var dataRows = {{dataRowAwait}}{{memberType.ToCSharp()}}.{{memberName}}{{parameters}};
				if (dataRows == null)
					throw new global::Xunit.Sdk.TestPipelineException("Test data returned null for {{classSymbol.ToDisplayString()}}.{{methodSymbol.Name}}. Make sure it is statically initialized before this test method is called.");
				{{foreachAwait}}foreach (var dataRow in dataRows)
					result.Add(attr.CreateDataRow(dataRow));
				return result;
			}
			""");

		result.Factories.Add((factory.ToString(), disableDiscoveryEnumeration));
	}
}
