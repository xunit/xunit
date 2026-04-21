using System.Text;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

public abstract class DataAttributeGeneratorBase(string fullyQualifiedAttributeType) :
	XunitAttributeGenerator<DataAttributeGeneratorBase.GeneratorResult>(fullyQualifiedAttributeType)
{
	protected override sealed void CreateSource(
		SourceProductionContext context,
		GeneratorResult result)
	{
		if (result is null || result.Factories.Count == 0)
			return;

		var initialization = new StringBuilder();

		foreach (var factory in result.Factories)
			// The extra whitespace around {{factory}} allows us to use preprocessor directives in the factory code
			initialization.Append($$"""
				global::Xunit.v3.RegisteredEngineConfig.RegisterTheoryDataRowFactory({{result.Type.Quoted()}}, {{result.MethodName.Quoted()}}, {{factory.DisableDiscoveryEnumeration.ToCSharp()}},
					{{factory.Factory.Replace("\n", "\n\t")}}
				);

				"""
			);

		AddInitAttribute(context, result, initialization.ToString());
	}

	protected static string? GetDataAttributeRegistration(
		AttributeData attribute,
		ITypeSymbol classSymbol)
	{
		Guard.ArgumentNotNull(attribute);

		var initializers = new List<string>();
		var skipType = default(ITypeSymbol);
		var skipUnless = default(string);
		var skipWhen = default(string);

		foreach (var namedArgument in attribute.NamedArguments)
		{
			switch (namedArgument.Key)
			{
				case Names.Xunit.v3.DataAttribute.Explicit:
					if (namedArgument.Value.Value is bool @explicit)
						initializers.Add($"Explicit = {@explicit.ToCSharp()}");
					break;

				case Names.Xunit.v3.DataAttribute.Label:
					if (namedArgument.Value.Value is string label)
						initializers.Add($"Label = {label.Quoted()}");
					break;

				case Names.Xunit.v3.DataAttribute.Skip:
					if (namedArgument.Value.Value is string skip)
						initializers.Add($"Skip = {skip.Quoted()}");
					break;

				case Names.Xunit.v3.DataAttribute.SkipType:
					skipType = namedArgument.Value.Value as ITypeSymbol;
					break;

				case Names.Xunit.v3.DataAttribute.SkipUnless:
					skipUnless = namedArgument.Value.Value as string;
					break;

				case Names.Xunit.v3.DataAttribute.SkipWhen:
					skipWhen = namedArgument.Value.Value as string;
					break;

				case Names.Xunit.v3.DataAttribute.TestDisplayName:
					if (namedArgument.Value.Value is string testDisplayName)
						initializers.Add($"TestDisplayName = {testDisplayName.Quoted()}");
					break;

				case Names.Xunit.v3.DataAttribute.Timeout:
					if (namedArgument.Value.Value is int timeout)
						initializers.Add($"Timeout = {timeout}");
					break;

				case Names.Xunit.v3.DataAttribute.Traits:
					if (namedArgument.Value.Kind == TypedConstantKind.Array)
					{
						var traitsArray = namedArgument.Value.Values.Select(c => c.Value as string).WhereNotNull().ToArray();
						var traits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
						var idx = 0;

						while (idx < traitsArray.Length - 1)
						{
							traits.AddOrGet(traitsArray[idx]).Add(traitsArray[idx + 1]);
							idx += 2;
						}

						if (traits.Count != 0)
						{
							var initializer = new StringBuilder("Traits = new global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.IReadOnlyCollection<string>>(global::System.StringComparer.OrdinalIgnoreCase) {");

							foreach (var kvp in traits)
								initializer.AppendFormat(CultureInfo.InvariantCulture, "[{0}] = new HashSet<string> {{ {1} }}", kvp.Key.Quoted(), string.Join(",", kvp.Value.Select(v => v.Quoted())));

							initializer.Append('}');
							initializers.Add(initializer.ToString());
						}
					}
					break;
			}
		}

		if (skipUnless is not null && skipWhen is not null)
			return null;
		if (!verifySkipProperty(skipUnless) || !verifySkipProperty(skipWhen))
			return null;

		if (skipUnless is not null)
			initializers.Add($"SkipUnless = () => {(skipType ?? classSymbol).ToCSharp()}.{skipUnless}");
		if (skipWhen is not null)
			initializers.Add($"SkipWhen = () => {(skipType ?? classSymbol).ToCSharp()}.{skipWhen}");

		return
			initializers.Count == 0
				? "global::Xunit.v3.DataAttributeRegistration.Empty"
				: $"new global::Xunit.v3.DataAttributeRegistration() {{ {string.Join(", ", initializers)} }}";

		bool verifySkipProperty(string? propertyName)
		{
			if (propertyName is null)
				return true;

			var currentSymbol = skipType ?? classSymbol;

			while (currentSymbol is not null)
			{
				var property =
					currentSymbol
						.GetMembers()
						.OfType<IPropertySymbol>()
						.FirstOrDefault(symbol => symbol.Name == propertyName);

				if (property is not null)
					return property.IsStatic && property.DeclaredAccessibility == Accessibility.Public && property.Type.ToCSharp() == "bool";

				currentSymbol = currentSymbol.BaseType;
			}

			return false;
		}
	}

	protected abstract void ProcessAttribute(
		INamedTypeSymbol classSymbol,
		IMethodSymbol methodSymbol,
		AttributeData attribute,
		string dataAttributeRegistration,
		GeneratorResult result,
		CancellationToken cancellationToken);

	protected override GeneratorResult? Transform(
		GeneratorAttributeSyntaxContext context,
		CancellationToken cancellationToken)
	{
		if (context.TargetSymbol is not IMethodSymbol methodSymbol || methodSymbol.ContainingType is not INamedTypeSymbol classSymbol)
			return null;

		var registrationType =
			classSymbol.IsGenericType
				? classSymbol.ConstructUnboundGenericType()
				: classSymbol;

		var result = new GeneratorResult(context) { Type = registrationType.ToCSharp(), MethodName = methodSymbol.Name };

		foreach (var attribute in context.Attributes)
			if (GetDataAttributeRegistration(attribute, classSymbol) is string dataAttributeRegistration)
				ProcessAttribute(classSymbol, methodSymbol, attribute, dataAttributeRegistration, result, cancellationToken);

		return result.Factories.Count == 0 ? null : result;
	}

	public class GeneratorResult(GeneratorAttributeSyntaxContext context) :
		XunitGeneratorResult(context.SemanticModel, context.TargetNode), IEquatable<GeneratorResult?>
	{
		public List<(string Factory, bool DisableDiscoveryEnumeration)> Factories = [];

		public required string MethodName { get; set; }

		public INamedTypeSymbol ObjectType { get; } =
			context.SemanticModel.Compilation.GetSpecialType(SpecialType.System_Object);

		public required string Type { get; set; }

		public override bool Equals(object? obj) =>
			Equals(obj as GeneratorResult);

		public bool Equals(GeneratorResult? other) =>
			other is not null &&
			base.Equals(other) &&
			ComparerHelper.Equals(Factories, other.Factories) &&
			ComparerHelper.Equals(MethodName, other.MethodName) &&
			ComparerHelper.Equals(Type, other.Type);

		public override int GetHashCode() =>
			Hasher.Extend(base.GetHashCode()).With(Factories).With(MethodName).With(Type);
	}
}
