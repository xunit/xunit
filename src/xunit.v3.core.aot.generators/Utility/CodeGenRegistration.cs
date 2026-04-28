using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

internal static class CodeGenRegistration
{
	static string ToConstructorInvocation(
		StringBuilder factoryBuilder,
		IMethodSymbol ctor,
		INamedTypeSymbol type,
		string typeDescription,
		string argumentLookupFormat,
		string objectFactoryFormat)
	{
		var testClassTypeName = type.ToCSharp();
		var parameterNamesInCode = new List<string>();

		if (ctor.Parameters.Length != 0)
		{
			var anyRequired = ctor.Parameters.Any(p => !p.IsOptional && !p.IsParams);

			if (anyRequired)
				factoryBuilder.Append("""
						var missingParameters = new global::System.Collections.Generic.List<(string Type, string Name)>();

					""");

			for (var idx = 0; idx < ctor.Parameters.Length; ++idx)
			{
				var parameter = ctor.Parameters[idx];
				var parameterName = parameter.Name.Quoted();
				var parameterNameInCode = $"param{idx}";
				parameterNamesInCode.Add(parameterNameInCode);

				factoryBuilder.Append($$"""
						var {{parameterNameInCode}} = await {{string.Format(CultureInfo.InvariantCulture, argumentLookupFormat, parameter.Type.ToCSharp())}};
						if (!{{parameterNameInCode}}.Success)

					""");

				if (parameter.IsOptional)
				{
					var defaultValue = parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue : null;
					factoryBuilder.Append($$"""
								{{parameterNameInCode}}.Result = {{defaultValue.QuotedIfString() ?? $"default({parameter.Type.ToCSharp(includeGlobal: false)})"}};

						""");
				}
				else if (parameter.IsParams)
					factoryBuilder.Append($$"""
								{{parameterNameInCode}}.Result = [];

						""");
				else
					factoryBuilder.Append($$"""
								missingParameters.Add(({{parameter.Type.ToDisplayString().Quoted()}}, {{parameterName}}));

						""");
			}

			if (anyRequired)
				factoryBuilder.Append($$"""
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

		factoryBuilder.Append($$"""
				var instance = new {{testClassTypeName}}({{string.Join(", ", parameterNamesInCode.Select(p => $"{p}.Result!"))}});

			""");

		factoryBuilder.Append($$"""
				return {{string.Format(CultureInfo.InvariantCulture, objectFactoryFormat, "instance")}};
			}
			""");

		return factoryBuilder.ToString();
	}

	internal static string ToFixtureFactories(IReadOnlyCollection<(string Type, string Factory)> fixtures) =>
		$$"""
		new global::System.Collections.Generic.Dictionary<global::System.Type, global::Xunit.v3.FixtureFactory> {
			{{string.Join(", ", fixtures.Select(f => $"[typeof({f.Type})] = {f.Factory.Replace("\n", "\n\t")}"))}}
		}
		""";

	internal static string? ToFixtureFactory(
		INamedTypeSymbol type,
		string typeDescription)
	{
		if (type.IsStatic || type.IsAbstract)
			return null;

		var publicCtors = type.Constructors.Where(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic).ToImmutableArray();
		if (publicCtors.Length != 1)
			return null;

		var factoryBuilder = new StringBuilder();
		factoryBuilder.Append("""
			async (mappingManager, forceCreation) => {

			""");

		if (!type.Implements(Types.Xunit.v3.INotifyLifecycle))
			factoryBuilder.Append("""
					if (!forceCreation)
						return null;

				""");

		return ToConstructorInvocation(
			factoryBuilder,
			publicCtors[0],
			type,
			typeDescription,
			"global::Xunit.v3.FixtureMappingManager.TryGetFixtureArgument<{0}>(mappingManager)",
			"{0}"
		);
	}

	// Use this when you don't know what ctor to call yet, but want to ensure there is only a single public non-static ctor
	internal static string? ToObjectFactory(
		INamedTypeSymbol type,
		string typeDescription)
	{
		if (type.IsStatic || type.IsAbstract)
			return null;

		var publicCtors = type.Constructors.Where(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic).ToImmutableArray();
		if (publicCtors.Length != 1)
			return null;

		var factoryBuilder = new StringBuilder();
		factoryBuilder.Append("""
			async mappingManager => {

			""");

		return ToConstructorInvocation(
			factoryBuilder,
			publicCtors[0],
			type,
			typeDescription,
			"mappingManager.TryGetFixtureArgument<{0}>()",
			"new global::Xunit.v3.CoreTestClassCreationResult({0})"
		);
	}

	// Use this when you already know the ctor you want to call
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

	public static string? ToOrdererFactory(
		AttributeData attribute,
		string requiredInterface)
	{
		var ordererType = default(INamedTypeSymbol);

		if (attribute.AttributeClass?.TypeArguments.Length == 1)
			ordererType = attribute.AttributeClass.TypeArguments[0] as INamedTypeSymbol;
		else if (attribute.ConstructorArguments.Length == 1)
			ordererType = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;

		if (ordererType is null)
			return null;

		var location = attribute.ApplicationSyntaxReference.Location;
		if (!ordererType.ImplementsInterface(requiredInterface))
			return null;

		var ctor = ordererType.Constructors.FirstOrDefault(c => c.Parameters.Length == 0);
		if (ctor is null)
			return null;

		return ToObjectFactory(ordererType, ctor);
	}

	public static string ToTraits(IReadOnlyDictionary<string, HashSet<string>> traits) =>
		$$"""
		new global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.IReadOnlyCollection<string>> {
			{{string.Join(", ", traits.Select(kvp => $"[{kvp.Key.Quoted()}] = new[] {{ {string.Join(", ", kvp.Value.Select(v => v.Quoted()))} }}"))}}
		}
		""";
}
