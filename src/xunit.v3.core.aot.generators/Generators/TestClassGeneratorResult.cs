using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

public class TestClassGeneratorResult(GeneratorSyntaxContext context) :
	XunitGeneratorResult(context.SemanticModel.SyntaxTree.FilePath, context.Node.GetLocation()), IEquatable<TestClassGeneratorResult?>
{
	public CodeGenTestClassRegistration? TestClass { get; set; }

	public required string TestClassType { get; set; }

	public List<CodeGenTestMethodRegistration> TestMethods = [];

	public override bool Equals(object? obj) =>
		Equals(obj as TestClassGeneratorResult);

	public bool Equals(TestClassGeneratorResult? other) =>
		other is not null &&
		base.Equals(other) &&
		ComparerHelper.Equal(TestClass, other.TestClass) &&
		ComparerHelper.Equal(TestClassType, other.TestClassType) &&
		ComparerHelper.Equal(TestMethods, other.TestMethods);

	public override int GetHashCode() =>
		HashCodeHelper.Extend(base.GetHashCode()).With(TestClass).With(TestClassType).With(TestMethods);
}
