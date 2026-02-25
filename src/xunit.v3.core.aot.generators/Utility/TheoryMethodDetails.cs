using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators;

public class TheoryMethodDetails : MethodDetailsBase
{
	public TheoryMethodDetails(
		INamedTypeSymbol classSymbol,
		MethodDeclarationSyntax methodDeclaration,
		IMethodSymbol methodSymbol,
		AttributeData attribute) :
			base(classSymbol, methodDeclaration, methodSymbol, attribute)
	{
		var requiredParameterCount = methodSymbol.Parameters.Where(p => !p.IsOptional && !p.IsParams).Count();

		var invokerFactoryBuilder = new StringBuilder();
		invokerFactoryBuilder.AppendLine($$"""
			async dataRow => {
				return async obj => {
					await using var disposalTracker = new global::Xunit.Sdk.DisposalTracker();
					var data = dataRow.GetData();
					disposalTracker.AddRange(data);
					if (data.Length < {{requiredParameterCount}})
						throw new global::Xunit.Sdk.TestPipelineException(
							string.Format(
								global::System.Globalization.CultureInfo.CurrentCulture,
								"The test method expected {{requiredParameterCount}} parameter value{{(requiredParameterCount == 1 ? "" : "s")}}, but {0} parameter value{1} {2} provided.",
								data.Length,
								data.Length == 1 ? "" : "s",
								data.Length == 1 ? "was" : "were"
							)
						);
			""");

		var anyOptional = false;
		var anyRequired = false;

		foreach (var parameter in methodSymbol.Parameters)
		{
			if (parameter.IsOptional || parameter.IsParams)
				anyOptional = true;
			else
				anyRequired = true;

			if (anyOptional && anyRequired)
				break;
		}

		if (anyRequired)
			invokerFactoryBuilder.AppendLine("""
							var invalidArguments = new global::System.Collections.Generic.List<(string Type, string Name, object? Value)>();
					""");
		if (anyOptional)
			ParameterDefaultValues = new string?[methodSymbol.Parameters.Length];

		var parameterNamesInCode = new List<string>();

		for (var idx = 0; idx < MethodSymbol.Parameters.Length; ++idx)
		{
			var parameter = MethodSymbol.Parameters[idx];
			var parameterName = parameter.Name.Quoted();
			var parameterNameInCode = "param" + idx;

			ParameterNames.Add(parameter.Name);
			parameterNamesInCode.Add(parameterNameInCode);

			var conversion = parameter.NullableAnnotation == NullableAnnotation.NotAnnotated ? "TryGet" : "TryGetNullable";
			invokerFactoryBuilder.AppendLine($$"""
						var {{parameterNameInCode}} = data.{{conversion}}<{{parameter.Type.ToCSharp()}}>({{idx}});
						if (!{{parameterNameInCode}}.Success)
				""");

			if (parameter.IsOptional)
			{
				var defaultValue = parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue : null;
				ParameterDefaultValues?[idx] = defaultValue.QuotedIfString() ?? (parameter.Type.IsValueType ? $"default({parameter.Type.ToDisplayString()})" : "null");
				invokerFactoryBuilder.AppendLine($$"""
								{{parameterNameInCode}}.Result = {{defaultValue.QuotedIfString() ?? $"default({parameter.Type.ToCSharp()})!"}};
					""");
			}
			else if (parameter.IsParams)
			{
				ParameterDefaultValues?[idx] = "[]";
				invokerFactoryBuilder.AppendLine($$"""
								{{parameterNameInCode}}.Result = [];
					""");
			}
			else
				invokerFactoryBuilder.AppendLine($$"""
								invalidArguments.Add(({{parameter.Type.ToDisplayString().Quoted()}}, {{parameterName}}, {{parameterNameInCode}}.RawValue));
					""");
		}

		if (anyRequired)
			invokerFactoryBuilder.AppendLine($$"""
						if (invalidArguments.Count != 0)
							throw new global::Xunit.Sdk.TestPipelineException(
								string.Format(
									global::System.Globalization.CultureInfo.CurrentCulture,
									"Test method had one or more invalid theory data arguments: {0}",
									string.Join(", ", global::System.Linq.Enumerable.Select(invalidArguments, a => $"{a.Type} {a.Name} ({a.Value ?? "null"})"))
								)
							);
				""");

		var paramsText = string.Join(", ", parameterNamesInCode.Select(p => $"{p}.Result!"));

		invokerFactoryBuilder.Append((classSymbol.IsStatic || MethodSymbol.IsStatic, MethodSymbol.ReturnType.SpecialType == SpecialType.System_Void) switch
		{
			// Static, returning void
			(true, true) => $"		{classSymbol.ToCSharp()}.{MethodSymbol.Name}({paramsText});",
			// Static, returning non-void
			(true, false) => $"		await global::Xunit.Sdk.AsyncUtility.Await({classSymbol.ToCSharp()}.{MethodSymbol.Name}({paramsText}));",
			// Non-static, returning void
			(false, true) => $"		(({classSymbol.ToCSharp()})obj!).{MethodSymbol.Name}({paramsText});",
			// Non-static, returning non-void
			(false, false) => $"		await global::Xunit.Sdk.AsyncUtility.Await((({classSymbol.ToCSharp()})obj!).{MethodSymbol.Name}({paramsText}));",
		});

		invokerFactoryBuilder.AppendLine("""

				};
			}
			""");

		MethodInvokerFactory = invokerFactoryBuilder.ToString();
	}

	public bool? DisableDiscoveryEnumeration { get; set; }

	public string MethodInvokerFactory { get; set; }

	public string?[]? ParameterDefaultValues { get; set; }

	public List<string> ParameterNames { get; } = [];

	public bool SkipTestWithoutData { get; set; }

	protected override void ProcessNamedArgument(
		string name,
		TypedConstant value)
	{
		switch (name)
		{
			case Names.Xunit.Internal.TheoryAttributeBase.DisableDiscoveryEnumeration:
				DisableDiscoveryEnumeration = value.Value is true;
				break;

			case Names.Xunit.Internal.TheoryAttributeBase.SkipTestWithoutData:
				SkipTestWithoutData = value.Value is true;
				break;

			default:
				base.ProcessNamedArgument(name, value);
				break;
		}
	}
}
