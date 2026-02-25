using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

public abstract class XunitAttributeGenerator<TResult>(string fullyQualifiedAttributeTypeName) :
	XunitGenerator
		where TResult : XunitGeneratorResult
{
	protected abstract void CreateSource(
		SourceProductionContext context,
		TResult result);

	protected override sealed void Initialize(
		IncrementalGeneratorInitializationContext context,
		IncrementalValueProvider<string> projectPath)
	{
		var result =
			context
				.SyntaxProvider
				.ForAttributeWithMetadataName(fullyQualifiedAttributeTypeName, ValidateAttribute, Transform)
				.WhereNotNull()
				.Combine(projectPath)
				.Select((pair, _) =>
				{
					pair.Left.ProjectPath = pair.Right;
					return pair.Left;
				});

		context.RegisterSourceOutput(result, Register);
	}

	void Register(
		SourceProductionContext context,
		TResult result)
	{
		foreach (var diagnostic in result.Diagnostics)
			context.ReportDiagnostic(diagnostic);

		CreateSource(context, result);
	}

	protected abstract TResult? Transform(
		GeneratorAttributeSyntaxContext context,
		CancellationToken cancellationToken);

	protected virtual bool ValidateAttribute(
		SyntaxNode syntaxNode,
		CancellationToken cancellationToken) =>
			true;
}
