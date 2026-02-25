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
			initialization.AppendLine($$"""
				global::Xunit.v3.RegisteredEngineConfig.RegisterTheoryDataRowFactory({{result.Type.Quoted()}}, {{result.MethodName.Quoted()}},
				{{factory}}
				);
				"""
			);

		AddInitAttribute(context, result, initialization.ToString());
	}

	protected static string? GetDataAttributeRegistration(
		AttributeData attribute,
		ITypeSymbol classSymbol,
		List<Diagnostic> diagnostics)
	{
		Guard.ArgumentNotNull(attribute);
		Guard.ArgumentNotNull(diagnostics);

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
		{
			diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X9006_CannotSetBothSkipUnlessAndSkipWhen,
					attribute.ApplicationSyntaxReference?.Location
				)
			);
			return null;
		}

		verifySkipProperty(skipUnless);
		verifySkipProperty(skipWhen);

		if (skipUnless is not null)
			initializers.Add($"SkipUnless = () => {(skipType ?? classSymbol).ToCSharp()}.{skipUnless}");
		if (skipWhen is not null)
			initializers.Add($"SkipWhen = () => {(skipType ?? classSymbol).ToCSharp()}.{skipWhen}");

		return
			diagnostics.Count != 0
				? null
				: initializers.Count == 0
					? "global::Xunit.v3.DataAttributeRegistration.Empty"
					: $"new global::Xunit.v3.DataAttributeRegistration() {{ {string.Join(", ", initializers)} }}";

		void verifySkipProperty(string? propertyName)
		{
			if (propertyName is null)
				return;

			var currentSymbol = skipType ?? classSymbol;

			while (currentSymbol is not null)
			{
				var property =
					currentSymbol
						.GetMembers()
						.OfType<IPropertySymbol>()
						.FirstOrDefault(symbol => symbol.Name == propertyName);

				if (property is not null)
				{
					if (property.Type.ToCSharp() == "bool")
						return;

					break;
				}

				currentSymbol = currentSymbol.BaseType;
			}

			diagnostics.Add(
				Diagnostic.Create(
					DiagnosticDescriptors.X9002_TypeMustHaveStaticPublicProperty,
					attribute.ApplicationSyntaxReference?.Location,
					skipType ?? classSymbol,
					propertyName,
					"bool"
				)
			);
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

		var result = new GeneratorResult(context) { Type = classSymbol.ToCSharp(), MethodName = methodSymbol.Name };

		foreach (var attribute in context.Attributes)
			if (GetDataAttributeRegistration(attribute, classSymbol, result.Diagnostics) is string dataAttributeRegistration)
				ProcessAttribute(classSymbol, methodSymbol, attribute, dataAttributeRegistration, result, cancellationToken);

		return result.Factories.Count == 0 && result.Diagnostics.Count == 0 ? null : result;
	}

	public class GeneratorResult(GeneratorAttributeSyntaxContext context) :
		XunitGeneratorResult(context.SemanticModel, context.TargetNode)
	{
		public Compilation Compilation { get; } =
			context.SemanticModel.Compilation;

		public List<string> Factories = [];

		public required string MethodName { get; set; }

		public required string Type { get; set; }
	}
}
