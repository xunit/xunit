using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

public class XunitGeneratorResult(
	SemanticModel model,
	SyntaxNode syntaxNode)
{
	// Ensure attributes have unique names based on the syntax tree where they're generated from
	readonly string baseSuffix = $"{model.SyntaxTree.FilePath}:{syntaxNode.GetLocation().SourceSpan.Start}".ToCompilerSafeName();

	public List<Diagnostic> Diagnostics { get; } = [];

	// Used to decorate type names to make them easier to identify in the generated types list
	// (for example, TestClassGenerator will put the class name into the generated attribute name)
	public string GeneratorSuffix { get; set; } = string.Empty;

	public string ProjectPath { get; set; } = string.Empty;

	public string SafeNameSuffix =>
		GeneratorSuffix + baseSuffix;
}
