using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

public abstract class RegisterRunnerReporterAttributeGeneratorBase(string fullyQualifiedAttributeTypeName) :
	XunitAttributeGenerator<RegisterRunnerReporterAttributeGeneratorBase.GeneratorResult>(fullyQualifiedAttributeTypeName)
{
	protected override string BaseInitAttributeName =>
		"global::Xunit.Runner.Common.RunnerInitializationAttribute";

	protected override void CreateSource(
		SourceProductionContext context,
		GeneratorResult result)
	{
		if (result is null)
			return;

		var code = new List<string>();

		foreach (var message in result.Messages)
			code.Add($"global::Xunit.Runner.Common.RegisteredRunnerConfig.RegisterRunnerReporterMessage({message.ToCSharp()});");

		foreach (var reporter in result.RunnerReporters.WhereNotNull())
			code.Add($"global::Xunit.Runner.Common.RegisteredRunnerConfig.RegisterRunnerReporter(new {reporter}());");

		if (code.Count == 0)
			return;

		AddInitAttribute(context, result, string.Join("\n", code));
	}

	protected virtual INamedTypeSymbol? GetTypeArgument(AttributeData attribute) =>
		FullyQualifiedAttributeTypeName.EndsWith("`1", StringComparison.Ordinal)
			? GetTypeArgumentGeneric(attribute)
			: GetTypeArgumentNonGeneric(attribute);

	protected static INamedTypeSymbol? GetTypeArgumentGeneric(AttributeData attribute)
	{
		if (attribute?.AttributeClass is not { } attributeType)
			return null;

		return
			attributeType.TypeArguments.Length == 1
				? attributeType.TypeArguments[0] as INamedTypeSymbol
				: null;
	}

	protected static INamedTypeSymbol? GetTypeArgumentNonGeneric(AttributeData attribute) =>
		attribute?.ConstructorArguments.Length == 1
			? attribute.ConstructorArguments[0].Value as INamedTypeSymbol
			: null;

	protected override GeneratorResult? Transform(
		GeneratorAttributeSyntaxContext context,
		CancellationToken cancellationToken)
	{
		if (context.TargetSymbol is not IAssemblySymbol)
			return null;

		var result = new GeneratorResult(context);

		foreach (var attribute in context.Attributes)
		{
			var reporterType = GetTypeArgument(attribute);
			if (reporterType is not null)
			{
				if (reporterType.HasParameterlessPublicCtor(out var _)
						&& reporterType.ImplementsInterface(Types.Xunit.Runner.Common.IRunnerReporter))
					result.RunnerReporters.Add(reporterType.ToString());
				else
					result.Messages.Add($"Runner reporter type '{reporterType.ToDisplayString()}' does not implement 'Xunit.Runner.Common.IRunnerReporter'");
			}
		}

		return result;
	}

	public sealed class GeneratorResult(GeneratorAttributeSyntaxContext context) :
		XunitGeneratorResult(context.SemanticModel.SyntaxTree.FilePath, context.TargetNode.GetLocation()), IEquatable<GeneratorResult?>
	{
		public List<string> Messages = [];

		public List<string?> RunnerReporters = [];

		public override bool Equals(object? obj) =>
			Equals(obj as GeneratorResult);

		public bool Equals(GeneratorResult? other) =>
			other is not null &&
			base.Equals(other) &&
			ComparerHelper.Equal(Messages, other.Messages) &&
			ComparerHelper.Equal(RunnerReporters, other.RunnerReporters);

		public override int GetHashCode() =>
			HashCodeHelper.Extend(base.GetHashCode()).With(Messages).With(RunnerReporters);
	}
}

[Generator(LanguageNames.CSharp)]
public class RegisterRunnerReporterAttributeGenerator() :
	RegisterRunnerReporterAttributeGeneratorBase(Types.Xunit.Runner.Common.RegisterRunnerReporterAttribute)
{ }


[Generator(LanguageNames.CSharp)]
public class RegisterRunnerReporterAttributeOfTGenerator() :
	RegisterRunnerReporterAttributeGeneratorBase(Types.Xunit.Runner.Common.RegisterRunnerReporterAttribute + "`1")
{ }
