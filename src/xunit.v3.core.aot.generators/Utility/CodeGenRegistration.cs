using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

internal static class CodeGenRegistration
{
	internal static string ToFixtureFactories(IReadOnlyCollection<(string Type, string Factory)> fixtures) =>
		$$"""
		new global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Func<global::Xunit.v3.FixtureMappingManager?, global::System.Threading.Tasks.ValueTask<object>>> {
			{{string.Join(", ", fixtures.Select(f => $"[typeof({f.Type})] = {f.Factory}"))}}
		}
		""";

	internal static string? ToFixtureFactory(
		INamedTypeSymbol type,
		Location? location,
		XunitGeneratorResult result,
		string typeDescription,
		string argumentLookupFormat,
		string objectFactoryFormat = "{0}")
	{
		if (type.IsStatic || type.IsAbstract)
			return null;

		var publicCtors = type.Constructors.Where(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic).ToImmutableArray();
		if (publicCtors.Length != 1)
		{
			result.Diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X9003_TypeMustHaveSinglePublicConstructor,
					location,
					typeDescription,
					type.ToDisplayString()
				)
			);
			return null;
		}

		var testClassTypeName = type.ToCSharp();
		var ctor = publicCtors[0];
		var parameterNamesInCode = new List<string>();

		var factoryBuilder = new StringBuilder();
		factoryBuilder.AppendLine("async mappingManager => {");

		if (ctor.Parameters.Length != 0)
		{
			var anyRequired = ctor.Parameters.Any(p => !p.IsOptional && !p.IsParams);

			if (anyRequired)
				factoryBuilder.AppendLine("""
						var missingParameters = new global::System.Collections.Generic.List<(string Type, string Name)>();
					""");

			for (var idx = 0; idx < ctor.Parameters.Length; ++idx)
			{
				var parameter = ctor.Parameters[idx];
				var parameterName = parameter.Name.Quoted();
				var parameterNameInCode = $"param{idx}";
				parameterNamesInCode.Add(parameterNameInCode);

				factoryBuilder.AppendLine($$"""
						var {{parameterNameInCode}} = await {{string.Format(CultureInfo.InvariantCulture, argumentLookupFormat, parameter.Type.ToCSharp())}};
						if (!{{parameterNameInCode}}.Success)
					""");

				if (parameter.IsOptional)
				{
					var defaultValue = parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue : null;
					factoryBuilder.AppendLine($$"""
								{{parameterNameInCode}}.Result = {{defaultValue.QuotedIfString() ?? $"default({parameter.Type.ToCSharp(includeGlobal: false)})"}};
						""");
				}
				else if (parameter.IsParams)
					factoryBuilder.AppendLine($$"""
								{{parameterNameInCode}}.Result = [];
						""");
				else
					factoryBuilder.AppendLine($$"""
								missingParameters.Add(({{parameter.Type.ToDisplayString().Quoted()}}, {{parameterName}}));
						""");
			}

			if (anyRequired)
				factoryBuilder.AppendLine($$"""
						if (missingParameters.Count != 0)
							throw new global::Xunit.Sdk.TestPipelineException(
								string.Format(
									global::System.Globalization.CultureInfo.CurrentCulture,
									"{{typeDescription}} '{{type}}' had one or more unresolved constructor arguments: {0}",
									string.Join(", ", global::System.Linq.Enumerable.Select(missingParameters, p => $"{p.Type} {p.Name}"))
								)
							);
					""");
		}

		factoryBuilder.AppendLine($$"""
				var instance = new {{testClassTypeName}}({{string.Join(", ", parameterNamesInCode.Select(p => $"{p}.Result!"))}});
			""");

		factoryBuilder.Append($$"""
				return {{string.Format(CultureInfo.InvariantCulture, objectFactoryFormat, "instance")}};
			}
			""");

		var factory = factoryBuilder.ToString();
		return factory;
	}

	public static string? ToObjectFactory(
		INamedTypeSymbol type,
		IMethodSymbol ctor)
	{
		if (!ctor.GetAttributes().Any(a => a.AttributeClass?.ToCSharp(includeGlobal: false) == Types.System.ObsoleteAttribute))
			return $"new {type.ToCSharp()}()";

		// Support our implicit "Instance" static that we use to prevent over-creation
		if (type.GetMembers("Instance").FirstOrDefault() is IPropertySymbol propertySymbol
				&& propertySymbol.IsStatic
				&& SymbolEqualityComparer.Default.Equals(propertySymbol.Type, type))
			return $"{type.ToCSharp()}.Instance";

		return null;
	}

	public static string ToTraits(IReadOnlyDictionary<string, List<string>> traits) =>
		$$"""
		new global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.IReadOnlyCollection<string>> {
			{{string.Join(", ", traits.Select(kvp => $"[{kvp.Key.Quoted()}] = new[] {{ {string.Join(", ", kvp.Value.Select(v => v.Quoted()))} }}"))}}
		}
		""";
}
