using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

public class TestClassGeneratorResult(GeneratorSyntaxContext context) :
	XunitGeneratorResult(context.SemanticModel, context.Node)
{
	public CodeGenTestClassRegistration? TestClass { get; set; }

	public required string TestClassType { get; set; }

	public Dictionary<string, (CodeGenTestMethodRegistration TestMethod, List<string> TestCaseFactories)> TestMethods = [];
}
