using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public class CollectionBehaviorAttributeGenerator() :
	XunitAttributeGenerator<CollectionBehaviorAttributeGenerator.GeneratorResult>(Types.Xunit.CollectionBehaviorAttribute)
{
	protected override sealed void CreateSource(
		SourceProductionContext context,
		GeneratorResult result)
	{
		if (result is null || result.CollectionFactoryType is null)
			return;

		AddInitAttribute(
			context, result,
			$"global::Xunit.v3.RegisteredEngineConfig.RegisterTestCollectionFactoryFactory((assembly) => new {result.CollectionFactoryType}(assembly));"
		);
	}

	protected override sealed GeneratorResult? Transform(
		GeneratorAttributeSyntaxContext context,
		CancellationToken cancellationToken)
	{
		if (context.TargetSymbol is not IAssemblySymbol)
			return null;

		var attribute = context.Attributes.FirstOrDefault();
		if (attribute is null)
			return null;

		var result = new GeneratorResult(context);

		if (attribute.ConstructorArguments.Length == 1)
		{
			var arg0Value = attribute.ConstructorArguments[0].Value;
			if (arg0Value is INamedTypeSymbol type)
			{
				var location = attribute.ApplicationSyntaxReference.Location;
				if (EnsureImplementsInterface(type, location, result, Types.Xunit.v3.ICodeGenTestCollectionFactory) &&
					EnsureConstructorParameters(type, location, result, [Types.Xunit.v3.ICodeGenTestAssembly]))
					result.CollectionFactoryType = type.ToCSharp();
			}
			else if (attribute.ConstructorArguments[0].Kind == TypedConstantKind.Enum)
				result.CollectionFactoryType = arg0Value switch
				{
					Values.Xunit.CollectionBehavior.CollectionPerAssembly => Types.Xunit.v3.CollectionPerAssemblyTestCollectionFactory,
					Values.Xunit.CollectionBehavior.CollectionPerClass => Types.Xunit.v3.CollectionPerClassTestCollectionFactory,
					_ => null,
				};
		}

		return result;
	}

	public sealed class GeneratorResult(GeneratorAttributeSyntaxContext context) :
		XunitGeneratorResult(context.SemanticModel, context.TargetNode)
	{
		public string? CollectionFactoryType { get; set; }
	}
}
