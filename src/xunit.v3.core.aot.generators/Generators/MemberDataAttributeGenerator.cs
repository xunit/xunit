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
			if (members.Length > 1)
			{
				result.Diagnostics.Add(
					Diagnostic.Create(
						DiagnosticDescriptors.X9012_MemberDataMemberCannotBeOverloaded,
						location,
						currentType.ToDisplayString(),
						memberName
					)
				);
				return;
			}

			if (currentType.DeclaredAccessibility is not Accessibility.Public and not Accessibility.Internal)
			{
				result.Diagnostics.Add(
					Diagnostic.Create(
						DiagnosticDescriptors.X9013_MemberDataTypeMustBePublicOrInternal,
						location,
						currentType.ToDisplayString(),
						memberName
					)
				);
				return;
			}

			member = members[0];
			break;
		}

		if (member is null)
		{
			result.Diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X1015_MemberDataMustReferenceExistingMember,
					location,
					memberName,
					memberType.ToDisplayString()
				)
			);
			return;
		}

		if (member.DeclaredAccessibility != Accessibility.Public)
		{
			result.Diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X1016_MemberDataMustReferencePublicMember,
					location
				)
			);
			return;
		}

		if (!member.IsStatic)
		{
			result.Diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X1017_MemberDataMustReferenceStaticMember,
					location
				)
			);
			return;
		}

		var returnType = member switch
		{
			IMethodSymbol method => method.ReturnType,
			IPropertySymbol property => property.Type,
			IFieldSymbol field => field.Type,
			_ => null,
		};

		if (returnType is null)
		{
			result.Diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X1018_MemberDataMustReferenceValidMemberKind,
					location
				)
			);
			return;
		}

		var theoryDataInfo = returnType.GetTheoryDataInfo(result.Compilation);
		if (theoryDataInfo is null)
		{
			result.Diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X1019_MemberDataMustReferenceMemberOfValidType,
					attribute.ApplicationSyntaxReference.Location,
					returnType
				)
			);
			return;
		}

		if (member is IPropertySymbol memberProperty)
			if (memberProperty.GetMethod is null || memberProperty.GetMethod.DeclaredAccessibility != Accessibility.Public)
			{
				result.Diagnostics.Add(
					Diagnostic.Create(
						DiagnosticDescriptors.X1020_MemberDataPropertyMustHaveGetter,
						attribute.ApplicationSyntaxReference.Location
					)
				);
				return;
			}

		var parameters = string.Empty;
		var parametersInit = new StringBuilder();
		if (member is IMethodSymbol memberMethod)
		{
			var arguments = attribute.ConstructorArguments[1].Values;

			if (arguments.Length > memberMethod.Parameters.Length)
			{
				result.Diagnostics.Add(
					Diagnostic.Create(
						DiagnosticDescriptors.X1036_MemberDataArgumentsMustMatchMethodParameters_ExtraValue,
						attribute.ApplicationSyntaxReference.Location,
						arguments[memberMethod.Parameters.Length].ToCSharp()
					)
				);
				return;
			}

			var paramsParameter = memberMethod.Parameters.FirstOrDefault(p => p.IsParams);
			if (paramsParameter is not null)
			{
				result.Diagnostics.Add(
					Diagnostic.Create(
						DiagnosticDescriptors.X9014_MemberDataParameterCannotBeParams,
						paramsParameter.Locations.FirstOrDefault(),
						paramsParameter.Name,
						memberType,
						memberName
					)
				);
				return;
			}

			var requiredParameters = memberMethod.Parameters.Where(p => !p.IsOptional).ToArray();
			if (arguments.Length < requiredParameters.Length)
			{
				result.Diagnostics.Add(
					Diagnostic.Create(
						DiagnosticDescriptors.X9015_MemberDataParameterCannotBeParams,
						attribute.ApplicationSyntaxReference.Location,
						memberType,
						memberName,
						requiredParameters[arguments.Length].Type,
						requiredParameters[arguments.Length].Name
					)
				);
				return;
			}

			var parameterNamesInCode = new List<string>();

			if (arguments.Length > 0)
			{
				parametersInit.AppendLine("""
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
							parametersInit.AppendLine($$"""
									invalidParameters.Add(({{parameter.Type.ToDisplayString().Quoted()}}, {{parameterName}}, "<missing value>"));
								""");
					}
					else
					{
						var argument = arguments[idx];
						var conversion = parameter.NullableAnnotation == NullableAnnotation.NotAnnotated ? "TryConvert" : "TryConvertNullable";

						parameterNamesInCode.Add(parameterNameInCode);
						parametersInit.AppendLine($$"""
								if (!global::Xunit.Sdk.TypeHelper.{{conversion}}({{argument.ToCSharp()}}, out {{parameter.Type.ToCSharp()}} {{parameterNameInCode}}))
									invalidParameters.Add(({{parameter.Type.ToDisplayString().Quoted()}}, {{parameterName}}, {{argument.ToCSharp().Quoted()}}));
							""");
					}
				}

				parametersInit.AppendLine($$"""
						if (invalidParameters.Count != 0)
							throw new global::Xunit.Sdk.TestPipelineException(
								string.Format(
									global::System.Globalization.CultureInfo.CurrentCulture,
									"Member data method '{{memberType.ToDisplayString()}}.{{memberMethod.Name}}' had one or more invalid theory data arguments: {0}",
									string.Join(", ", global::System.Linq.Enumerable.Select(invalidParameters, a => $"{a.Type} {a.Name} ({a.Value})"))
								)
							);
					""");
			}

			parameters = $"({string.Join(", ", parameterNamesInCode)})";
		}

		result.GeneratorSuffix = $"{classSymbol.Name}٠{methodSymbol.Name}٠";

		var factory = new StringBuilder();

		var span = location?.SourceTree?.GetLineSpan(location.SourceSpan, cancellationToken);
		if (span.HasValue)
			factory.AppendLine($@"#line {span.Value.StartLinePosition.Line - 1} ""{span.Value.Path}""");

		var foreachAwait = theoryDataInfo.Value.IsAsyncEnumerable ? "await " : "";
		var dataRowAwait = theoryDataInfo.Value.IsTask ? "await " : "";

		factory.AppendLine($$"""
			async disposalTracker => {
			{{parametersInit}}
				var attr = {{dataAttributeRegistration}};
				var result = new global::System.Collections.Generic.List<global::Xunit.ITheoryDataRow>();
				var dataRows = {{dataRowAwait}}{{memberType.ToCSharp()}}.{{memberName}}{{parameters}};
				if (dataRows == null)
					throw new global::Xunit.Sdk.TestPipelineException("Test data returned null for {{classSymbol.ToDisplayString()}}.{{methodSymbol.Name}}. Make sure it is statically initialized before this test method is called.");
				{{foreachAwait}}foreach (var dataRow in dataRows)
					result.Add(attr.CreateDataRow(dataRow));
				return result;
			}
			""");

		if (span.HasValue)
			factory.AppendLine("#line default");

		result.Factories.Add(factory.ToString());
	}
}
